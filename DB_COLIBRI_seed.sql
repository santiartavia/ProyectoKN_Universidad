-- ======================================================================
-- DATOS INICIALES (SEED) - SOLO SI LA TABLA ESTA VACIA
-- ======================================================================

-- Roles base
IF NOT EXISTS (SELECT 1 FROM dbo.Roles)
BEGIN
    INSERT INTO dbo.Roles (nombre_rol, descripcion) VALUES
        (N'Administrador', N'Acceso total al sistema'),
        (N'Cajero',        N'Gestión de ventas y cierres de caja'),
        (N'Mesero',        N'Toma y gestión de pedidos en sala'),
        (N'Cocinero',      N'Visualización y actualización de pedidos en cocina'),
        (N'Supervisor',    N'Reportes y supervisión operativa');
END
GO

-- Categorias de insumo
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Insumo)
BEGIN
    INSERT INTO dbo.Categorias_Insumo (nombre_categoria) VALUES
        (N'Bebidas'), (N'Carnes'), (N'Lácteos'), (N'Vegetales'),
        (N'Granos y cereales'), (N'Condimentos'), (N'Postres'), (N'Limpieza');
END
GO

-- Categorias de producto
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Producto)
BEGIN
    INSERT INTO dbo.Categorias_Producto (nombre_categoria) VALUES
        (N'Entradas'), (N'Platos Fuertes'), (N'Pastas'), (N'Ensaladas'),
        (N'Postres'), (N'Bebidas frías'), (N'Bebidas calientes'), (N'Combos'),
        (N'Menú Principal');
END
GO

-- Cajas
IF NOT EXISTS (SELECT 1 FROM dbo.Cajas)
BEGIN
    INSERT INTO dbo.Cajas (nombre_caja, estado_caja) VALUES
        (N'Caja Principal Terminal 01', N'cerrada'),
        (N'Caja Principal Terminal 02', N'cerrada');
END
GO

-- Mesas
IF NOT EXISTS (SELECT 1 FROM dbo.Mesas)
BEGIN
    INSERT INTO dbo.Mesas (numero_mesa, capacidad, estado_mesa) VALUES
        (N'1', 4, N'disponible'), (N'2', 4, N'disponible'),
        (N'3', 4, N'disponible'), (N'4', 4, N'disponible'),
        (N'5', 4, N'disponible'), (N'6', 4, N'disponible'),
        (N'7', 4, N'disponible'), (N'8', 4, N'disponible'),
        (N'9', 4, N'disponible'), (N'10', 4, N'disponible'),
        (N'11', 4, N'disponible'), (N'12', 4, N'disponible'),
        (N'13', 6, N'disponible');
END
GO

-- Proveedores
IF NOT EXISTS (SELECT 1 FROM dbo.Proveedores)
BEGIN
    INSERT INTO dbo.Proveedores (cedula_juridica, nombre_empresa, contacto_nombre, telefono, correo) VALUES
        (N'3-101-123456', N'Distribuidora Alimentos del Valle S.A.',       N'Carlos Mora',     N'2256-7890', N'carlos.mora@alivalle.com'),
        (N'3-102-654321', N'Carnes Premium del Pacífico S.R.L.',           N'María Jiménez',   N'2288-4567', N'maria.jimenez@carnespremium.cr'),
        (N'3-103-987654', N'Lácteos Monteverde S.A.',                      N'Ana Rodríguez',   N'2267-8901', N'ana.rodriguez@monteverde.com'),
        (N'3-104-111222', N'Bebidas y Refrescos Nacionales S.A.',          N'Pedro Vargas',    N'2276-5432', N'pedro.vargas@bebidasnac.cr'),
        (N'3-105-333444', N'Granos y Abarrotes Doña Tere S.R.L.',          N'Teresa Castillo', N'2258-9012', N'teresa.castillo@granosdotere.com'),
        (N'3-106-555666', N'Condimentos y Especias Finas S.A.',            N'Luis Chacón',     N'2234-5678', N'luis.chacon@condimentosfinas.com'),
        (N'3-107-777888', N'Distribuidora de Frutas y Vegetales Frescos',  N'Rosa Núñez',      N'2290-3456', N'rosa.nunez@freshveg.cr'),
        (N'3-108-999000', N'Proveedora de Limpieza Profesional S.A.',      N'José Sandí',      N'2250-7890', N'jose.sandi@limpiezaprofesional.com');
END
GO

-- Usuarios demo (password de todos: Password1234.)
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = N'admin')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES (1, N'admin', N'admin@colibri.com', N'zV6JT6+5WSY1wJ48nj8yn7EHQ8rBJHPrpu488+KNdYQWKYlMtR9pQUXPj++RyTTo', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, fecha_ingreso, estado)
    VALUES (SCOPE_IDENTITY(), N'0-0000-0000', N'Administrador', N'Sistema', 5000, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = N'cajero_demo')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES (2, N'cajero_demo', N'cajero@colibri.com', N'OhxNO55QZdrhUUuRGuC1fAtu1uCv8mGHK5cbPFUPsDHuIanqGRT0jHqbDjqN5lMV', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, fecha_ingreso, estado)
    VALUES (SCOPE_IDENTITY(), N'2-2222-2222', N'Juan', N'Cajero Demo', 2500, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = N'mesero_demo')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES (3, N'mesero_demo', N'mesero@colibri.com', N'IjeGt57fg2wTSBJrIQqCfnDOBEF6LRp3kQVr32gnLcbagnhl1AL8yA6gpGJ0g6ya', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, fecha_ingreso, estado)
    VALUES (SCOPE_IDENTITY(), N'3-3333-3333', N'María', N'Mesero Demo', 2500, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = N'cocinero_demo')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES (4, N'cocinero_demo', N'cocinero@colibri.com', N'pEtqmzN3xGL6jzuKRK/lZ9hsBIbwl6phuXykbTmQd54z5XxTvKK+KrGWaq7QK5o5', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, fecha_ingreso, estado)
    VALUES (SCOPE_IDENTITY(), N'4-4444-4444', N'Carlos', N'Cocinero Demo', 3000, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = N'supervisor_demo')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES (5, N'supervisor_demo', N'supervisor@colibri.com', N'NiEyjByddIGBvYbwCgpkF/LvtgwiU9XIdqsehf8KFJvpyx9fCfDoyWxeBcLH6Dlg', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, fecha_ingreso, estado)
    VALUES (SCOPE_IDENTITY(), N'5-5555-5555', N'Laura', N'Supervisor Demo', 3500, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO
