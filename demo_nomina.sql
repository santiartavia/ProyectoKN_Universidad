USE COLIBRI;
GO

-- =====================================================
-- DEMO NÓMINA MENSUAL — Junio 2026
-- =====================================================
-- Requisitos:
--   1. Ejecutar primero nomina_mensual.sql si no existe
--   2. Roles ya deben estar insertados (1..5)
-- =====================================================

-- 1. CREAR ADMIN (si no existe en Empleados)
IF NOT EXISTS (SELECT 1 FROM dbo.Empleados WHERE id_usuario = 1003)
BEGIN
    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, telefono, correo_personal, salario_hora, dias_vacaciones_disponibles, fecha_ingreso, estado)
    VALUES (1003, '0-0000-0000', 'Administrador', 'Sistema', NULL, NULL, 5000, 0, CAST(SYSUTCDATETIME() AS DATE), 1);
    PRINT 'Admin insertado en Empleados.';
END
GO

-- 2. CREAR USUARIOS DEMO
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = 'jcajero')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES (2, 'jcajero', 'juan@colibri.com', 'ZGVtbw==', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = 'mmesero')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES (3, 'mmesero', 'maria@colibri.com', 'ZGVtbw==', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = 'ccocinero')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES (4, 'ccocinero', 'carlos@colibri.com', 'ZGVtbw==', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME());
END
GO

-- 3. CREAR EMPLEADOS DEMO (asumiendo identity sequence ~1004+ para usuarios nuevos)
DECLARE @uid_juan INT, @uid_maria INT, @uid_carlos INT;
SELECT @uid_juan   = id_usuario FROM dbo.Usuarios WHERE nombre_usuario = 'jcajero';
SELECT @uid_maria  = id_usuario FROM dbo.Usuarios WHERE nombre_usuario = 'mmesero';
SELECT @uid_carlos = id_usuario FROM dbo.Usuarios WHERE nombre_usuario = 'ccocinero';

IF NOT EXISTS (SELECT 1 FROM dbo.Empleados WHERE id_usuario = @uid_juan)
    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, telefono, correo_personal, salario_hora, dias_vacaciones_disponibles, fecha_ingreso, estado)
    VALUES (@uid_juan, '1-1111-1111', 'Juan', 'Pérez', '8888-1111', 'juan@example.com', 3500, 15, '2024-01-15', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Empleados WHERE id_usuario = @uid_maria)
    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, telefono, correo_personal, salario_hora, dias_vacaciones_disponibles, fecha_ingreso, estado)
    VALUES (@uid_maria, '2-2222-2222', 'María', 'López', '8888-2222', 'maria@example.com', 2800, 12, '2024-06-01', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Empleados WHERE id_usuario = @uid_carlos)
    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, telefono, correo_personal, salario_hora, dias_vacaciones_disponibles, fecha_ingreso, estado)
    VALUES (@uid_carlos, '3-3333-3333', 'Carlos', 'Mora', '8888-3333', 'carlos@example.com', 3200, 10, '2024-03-10', 1);
GO

-- 4. INSERTAR ASISTENCIAS — Junio 2026 (días hábiles)
-- Jornada: entrada 8:00, salida 17:00 (9h/día, 1h almuerzo = 8h trabajadas)
-- Se usa CTE para generar los días hábiles de junio 2026

DECLARE @e_admin INT, @e_juan INT, @e_maria INT, @e_carlos INT;
SELECT @e_admin  = id_empleado FROM dbo.Empleados WHERE cedula = '0-0000-0000';
SELECT @e_juan   = id_empleado FROM dbo.Empleados WHERE cedula = '1-1111-1111';
SELECT @e_maria  = id_empleado FROM dbo.Empleados WHERE cedula = '2-2222-2222';
SELECT @e_carlos = id_empleado FROM dbo.Empleados WHERE cedula = '3-3333-3333';

-- Tabla temporal con días hábiles de junio 2026
DECLARE @dias TABLE (fecha DATE PRIMARY KEY);
DECLARE @d DATE = '2026-06-01';
WHILE @d <= '2026-06-30'
BEGIN
    IF DATEPART(WEEKDAY, @d) NOT IN (1, 7)  -- lunes=2, martes=3, ..., sábado=7, domingo=1  (@@DATEFIRST = 7 default US)
        INSERT INTO @dias VALUES (@d);
    SET @d = DATEADD(DAY, 1, @d);
END

-- 4a. Asistencias de Juan (todos los días hábiles)
INSERT INTO dbo.Asistencia (id_empleado, id_turno, fecha_hora_entrada, fecha_hora_salida, observaciones, estado)
SELECT @e_juan, NULL,
       CAST(fecha AS DATETIME) + CAST('08:00:00' AS DATETIME),
       CAST(fecha AS DATETIME) + CAST('17:00:00' AS DATETIME),
       NULL, 1
FROM @dias
WHERE NOT EXISTS (SELECT 1 FROM dbo.Asistencia a WHERE a.id_empleado = @e_juan AND CAST(a.fecha_hora_entrada AS DATE) = fecha);

-- 4b. Asistencias de María (sin vacaciones: Jun 1-5 y Jun 15-30)
INSERT INTO dbo.Asistencia (id_empleado, id_turno, fecha_hora_entrada, fecha_hora_salida, observaciones, estado)
SELECT @e_maria, NULL,
       CAST(fecha AS DATETIME) + CAST('08:00:00' AS DATETIME),
       CAST(fecha AS DATETIME) + CAST('17:00:00' AS DATETIME),
       NULL, 1
FROM @dias
WHERE fecha NOT BETWEEN '2026-06-08' AND '2026-06-12'
  AND NOT EXISTS (SELECT 1 FROM dbo.Asistencia a WHERE a.id_empleado = @e_maria AND CAST(a.fecha_hora_entrada AS DATE) = fecha);

-- 4c. Asistencias de Carlos (todos los días hábiles)
INSERT INTO dbo.Asistencia (id_empleado, id_turno, fecha_hora_entrada, fecha_hora_salida, observaciones, estado)
SELECT @e_carlos, NULL,
       CAST(fecha AS DATETIME) + CAST('10:00:00' AS DATETIME),
       CAST(fecha AS DATETIME) + CAST('19:00:00' AS DATETIME),
       NULL, 1
FROM @dias
WHERE NOT EXISTS (SELECT 1 FROM dbo.Asistencia a WHERE a.id_empleado = @e_carlos AND CAST(a.fecha_hora_entrada AS DATE) = fecha);

PRINT 'Asistencias insertadas.';

-- 5. HORAS EXTRA (Juan — 4 horas extra en días específicos)
DECLARE @asistencia_he TABLE (id_asistencia INT, fecha DATE);
INSERT INTO @asistencia_he
SELECT TOP 4 id_asistencia, CAST(fecha_hora_entrada AS DATE)
FROM dbo.Asistencia
WHERE id_empleado = @e_juan
  AND CAST(fecha_hora_entrada AS DATE) IN ('2026-06-03','2026-06-10','2026-06-17','2026-06-24')
ORDER BY fecha_hora_entrada;

INSERT INTO dbo.Horas_Extra (id_asistencia, cantidad_horas, factor_pago, monto_calculado, motivo_ajuste, estado)
SELECT ah.id_asistencia, 1.5, 1.50, 1.5 * 3500 * 1.50, 'Horas extra demo', 1
FROM @asistencia_he ah
WHERE NOT EXISTS (SELECT 1 FROM dbo.Horas_Extra h WHERE h.id_asistencia = ah.id_asistencia);

PRINT 'Horas extra insertadas.';

-- 6. VACACIONES APROBADAS (María — 5 días: Jun 8-12)
IF NOT EXISTS (SELECT 1 FROM dbo.Vacaciones WHERE id_empleado = @e_maria AND fecha_inicio = '2026-06-08')
BEGIN
    INSERT INTO dbo.Vacaciones (id_empleado, id_aprobador, fecha_inicio, fecha_fin, dias_solicitados, estado_solicitud, fecha_solicitud, estado)
    VALUES (@e_maria, @e_admin, '2026-06-08', '2026-06-12', 5, 'aprobada', SYSUTCDATETIME(), 1);
    PRINT 'Vacaciones aprobadas para María.';
END
GO

-- 7. MOSTRAR RESUMEN
SELECT 'EMPLEADOS' AS seccion, e.id_empleado, e.nombre, e.apellidos, e.salario_hora, e.dias_vacaciones_disponibles
FROM dbo.Empleados e;

SELECT 'ASISTENCIAS (Junio 2026)' AS seccion, e.nombre + ' ' + e.apellidos AS empleado, COUNT(*) AS dias,
       COUNT(CASE WHEN a.fecha_hora_salida IS NOT NULL THEN 1 END) AS con_salida
FROM dbo.Asistencia a
JOIN dbo.Empleados e ON e.id_empleado = a.id_empleado
WHERE a.fecha_hora_entrada >= '2026-06-01' AND a.fecha_hora_entrada < '2026-07-01'
GROUP BY e.id_empleado, e.nombre, e.apellidos;

SELECT 'HORAS EXTRA' AS seccion, e.nombre + ' ' + e.apellidos AS empleado, SUM(h.cantidad_horas) AS total_horas
FROM dbo.Horas_Extra h
JOIN dbo.Asistencia a ON a.id_asistencia = h.id_asistencia
JOIN dbo.Empleados e ON e.id_empleado = a.id_empleado
GROUP BY e.id_empleado, e.nombre, e.apellidos;

SELECT 'VACACIONES APROBADAS' AS seccion, e.nombre + ' ' + e.apellidos AS empleado,
       v.fecha_inicio, v.fecha_fin, v.dias_solicitados
FROM dbo.Vacaciones v
JOIN dbo.Empleados e ON e.id_empleado = v.id_empleado
WHERE v.estado_solicitud = 'aprobada';
GO

PRINT '';
PRINT '=====================================================';
PRINT 'DEMO LISTA — Abre Nómina Mensual → mes=6, año=2026';
PRINT '=====================================================';
PRINT 'Usuarios creados (pass: demo, cambio requerido):';
PRINT '  - jcajero   (Juan Pérez)';
PRINT '  - mmesero   (María López)';
PRINT '  - ccocinero (Carlos Mora)';
PRINT '=====================================================';
GO
