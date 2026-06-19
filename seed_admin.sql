USE COLIBRI;
GO

-- Insertar usuario admin inicial (password = "admin" en Base64)
INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
VALUES (1, 'admin', 'admin@colibri.com', 'YWRtaW4=', 1, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

-- Insertar empleado vinculado al usuario admin
INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, fecha_ingreso, estado)
VALUES (1, '0-0000-0000', 'Administrador', 'Sistema', 5000, CAST(SYSUTCDATETIME() AS DATE), 1);

PRINT 'Usuario admin creado: usuario=admin, password=admin';
GO
