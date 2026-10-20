-- ==========================================================================================
-- SCRIPT DE REPARACIÓN INTEGRAL DE BASE DE DATOS - RESTAURANTE EL COLIBRÍ
-- Nota: Este script es 100% idempotente (se puede ejecutar múltiples veces sin errores).
-- Además, ColibriDbContext ya ejecuta estas verificaciones automáticamente al iniciar.
-- ==========================================================================================

-- 1. Asegurar columnas ip_origen y dispositivo en tablas de bitácora
IF COL_LENGTH('dbo.Bitacora_Usuarios_RRHH', 'ip_origen') IS NULL
    ALTER TABLE dbo.Bitacora_Usuarios_RRHH ADD ip_origen VARCHAR(50) NULL;
IF COL_LENGTH('dbo.Bitacora_Usuarios_RRHH', 'dispositivo') IS NULL
    ALTER TABLE dbo.Bitacora_Usuarios_RRHH ADD dispositivo VARCHAR(250) NULL;

IF COL_LENGTH('dbo.Bitacora_Acceso', 'ip_origen') IS NULL
    ALTER TABLE dbo.Bitacora_Acceso ADD ip_origen VARCHAR(50) NULL;
IF COL_LENGTH('dbo.Bitacora_Acceso', 'dispositivo') IS NULL
    ALTER TABLE dbo.Bitacora_Acceso ADD dispositivo VARCHAR(250) NULL;

IF COL_LENGTH('dbo.Bitacora_Inventario', 'ip_origen') IS NULL
    ALTER TABLE dbo.Bitacora_Inventario ADD ip_origen VARCHAR(50) NULL;
IF COL_LENGTH('dbo.Bitacora_Inventario', 'dispositivo') IS NULL
    ALTER TABLE dbo.Bitacora_Inventario ADD dispositivo VARCHAR(250) NULL;

-- 2. Eliminar restricción restrictiva CK_BitRR_accion en Bitacora_Usuarios_RRHH
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_BitRR_accion')
BEGIN
    ALTER TABLE dbo.Bitacora_Usuarios_RRHH DROP CONSTRAINT CK_BitRR_accion;
END;

-- 3. Crear tabla Bitacora_Financiera si no existe
IF OBJECT_ID('dbo.Bitacora_Financiera', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bitacora_Financiera (
        id_registro INT IDENTITY(1,1) PRIMARY KEY,
        id_usuario INT NOT NULL,
        tabla_afectada VARCHAR(80) NOT NULL,
        id_registro_afectado INT NOT NULL,
        accion VARCHAR(30) NOT NULL,
        valor_anterior NVARCHAR(MAX) NULL,
        valor_nuevo NVARCHAR(MAX) NULL,
        detalle VARCHAR(300) NULL,
        ip_origen VARCHAR(50) NULL,
        dispositivo VARCHAR(250) NULL,
        fecha_hora DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Bitacora_Financiera', 'ip_origen') IS NULL
        ALTER TABLE dbo.Bitacora_Financiera ADD ip_origen VARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Bitacora_Financiera', 'dispositivo') IS NULL
        ALTER TABLE dbo.Bitacora_Financiera ADD dispositivo VARCHAR(250) NULL;
END;

-- 4. Crear tabla Bitacora_Reportes si no existe
IF OBJECT_ID('dbo.Bitacora_Reportes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Bitacora_Reportes (
        id_registro INT IDENTITY(1,1) PRIMARY KEY,
        id_usuario INT NOT NULL,
        accion VARCHAR(30) NOT NULL,
        valor_anterior NVARCHAR(MAX) NULL,
        valor_nuevo NVARCHAR(MAX) NULL,
        detalle VARCHAR(300) NULL,
        ip_origen VARCHAR(50) NULL,
        dispositivo VARCHAR(250) NULL,
        fecha_hora DATETIME2 NOT NULL DEFAULT SYSDATETIME()
    );
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Bitacora_Reportes', 'ip_origen') IS NULL
        ALTER TABLE dbo.Bitacora_Reportes ADD ip_origen VARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Bitacora_Reportes', 'dispositivo') IS NULL
        ALTER TABLE dbo.Bitacora_Reportes ADD dispositivo VARCHAR(250) NULL;
END;

-- 5. Crear tabla Cierres_Periodo si no existe
IF OBJECT_ID('dbo.Cierres_Periodo', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cierres_Periodo (
        id_cierre_periodo INT IDENTITY(1,1) PRIMARY KEY,
        id_usuario INT NOT NULL,
        tipo_periodo VARCHAR(20) NOT NULL,
        mes INT NULL,
        anio INT NOT NULL,
        fecha_cierre DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
        total_ingresos DECIMAL(12,2) NOT NULL DEFAULT 0,
        total_egresos DECIMAL(12,2) NOT NULL DEFAULT 0,
        total_notas_credito DECIMAL(12,2) NOT NULL DEFAULT 0,
        saldo_final DECIMAL(12,2) NOT NULL DEFAULT 0,
        estado BIT NOT NULL DEFAULT 1
    );
END;

-- 6. Asegurar que exista al menos una Caja activa
IF NOT EXISTS (SELECT 1 FROM dbo.Cajas WHERE estado = 1)
BEGIN
    INSERT INTO dbo.Cajas (nombre_caja, estado_caja, estado)
    VALUES ('Caja Principal', 'cerrada', 1);
END;

-- 7. Vincular Usuarios huérfanos con un registro en Empleados
INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, dias_vacaciones_disponibles, fecha_ingreso, estado)
SELECT u.id_usuario,
       LEFT('U' + CAST(u.id_usuario AS VARCHAR(10)) + '_' + u.nombre_usuario, 20),
       u.nombre_usuario,
       ISNULL(r.nombre_rol, 'Sistema'),
       1500.00,
       0,
       CAST(GETDATE() AS DATE),
       u.estado
FROM dbo.Usuarios u
LEFT JOIN dbo.Roles r ON u.id_rol = r.id_rol
WHERE NOT EXISTS (SELECT 1 FROM dbo.Empleados e WHERE e.id_usuario = u.id_usuario);
