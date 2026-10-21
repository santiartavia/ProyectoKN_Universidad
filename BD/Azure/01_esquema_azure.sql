-- 01 - ESQUEMA COMPLETO PARA AZURE SQL DATABASE
-- Ejecutar conectado DIRECTAMENTE a la base de la aplicación, ya creada en Azure.
-- Sin USE, CREATE DATABASE, GO, SQLCMD, usuarios del servidor ni semillas.
-- Crea tablas faltantes. No borra registros ni reemplaza tablas existentes.
-- Compatible con el esquema INSTALAR_BASE_DATOS_COMPLETA.sql del proyecto.
-- No es una migración universal de bases con columnas o restricciones diferentes.
SET NOCOUNT ON;
SET XACT_ABORT ON;
IF DB_NAME() IN (N'master', N'model', N'msdb', N'tempdb')
    THROW 51000, 'Seleccione la base de datos de la aplicación antes de ejecutar.', 1;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
BEGIN TRY
    BEGIN TRANSACTION;
    EXEC sys.sp_executesql N'-- Roles
IF OBJECT_ID(N''dbo.Roles'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Roles] (
    [id_rol] INT IDENTITY(1,1) NOT NULL,
    [nombre_rol] NVARCHAR(60) NOT NULL,
    [descripcion] NVARCHAR(255) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([id_rol]),
    CONSTRAINT [UQ_Roles_nombre] UNIQUE ([nombre_rol])
);
END';

    EXEC sys.sp_executesql N'-- Usuarios
IF OBJECT_ID(N''dbo.Usuarios'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Usuarios] (
    [id_usuario] INT IDENTITY(1,1) NOT NULL,
    [id_rol] INT NOT NULL,
    [nombre_usuario] NVARCHAR(80) NOT NULL,
    [correo] NVARCHAR(150) NOT NULL,
    [password_hash] NVARCHAR(255) NOT NULL,
    [cambio_password_requerido] BIT NOT NULL DEFAULT 0,
    [intentos_fallidos] TINYINT NOT NULL DEFAULT 0,
    [bloqueado] BIT NOT NULL DEFAULT 0,
    [fecha_ultimo_acceso] DATETIME2 NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    [fecha_creacion] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [fecha_password] DATETIME2 NULL,
    [fecha_aviso_password] DATETIME2 NULL,
    [ultimo_cambio_password_ip] NVARCHAR(50) NULL,
    [ultimo_cambio_password_dispositivo] NVARCHAR(100) NULL,
    [direccion] NVARCHAR(255) NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED ([id_usuario]),
    CONSTRAINT [UQ_Usuarios_nombre] UNIQUE ([nombre_usuario]),
    CONSTRAINT [FK_Usuarios_Rol] FOREIGN KEY ([id_rol]) REFERENCES [dbo].[Roles] ([id_rol])
);
END';

    EXEC sys.sp_executesql N'-- Empleados
IF OBJECT_ID(N''dbo.Empleados'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Empleados] (
    [id_empleado] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [cedula] NVARCHAR(20) NOT NULL,
    [nombre] NVARCHAR(80) NOT NULL,
    [apellidos] NVARCHAR(120) NOT NULL,
    [telefono] NVARCHAR(20) NULL,
    [correo_personal] NVARCHAR(150) NULL,
    [direccion] NVARCHAR(255) NULL,
    [salario_hora] DECIMAL(10,2) NOT NULL DEFAULT 1500.00,
    [dias_vacaciones_disponibles] DECIMAL(6,2) NOT NULL DEFAULT 0,
    [fecha_ingreso] DATE NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    [fecha_modificacion] DATETIME2 NULL,
    [fecha_reactivacion] DATETIME2 NULL,
    [motivo_inactivacion] NVARCHAR(300) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Empleados] PRIMARY KEY CLUSTERED ([id_empleado]),
    CONSTRAINT [UQ_Empleados_cedula] UNIQUE ([cedula]),
    CONSTRAINT [FK_Empleados_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END';

    EXEC sys.sp_executesql N'-- Mesas
IF OBJECT_ID(N''dbo.Mesas'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Mesas] (
    [id_mesa] INT IDENTITY(1,1) NOT NULL,
    [numero_mesa] NVARCHAR(10) NOT NULL,
    [capacidad] TINYINT NOT NULL DEFAULT 4,
    [estado_mesa] NVARCHAR(20) NOT NULL DEFAULT ''disponible'',
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Mesas] PRIMARY KEY CLUSTERED ([id_mesa]),
    CONSTRAINT [UQ_Mesas_numero] UNIQUE ([numero_mesa])
);
END';

    EXEC sys.sp_executesql N'-- Proveedores
IF OBJECT_ID(N''dbo.Proveedores'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Proveedores] (
    [id_proveedor] INT IDENTITY(1,1) NOT NULL,
    [cedula_juridica] NVARCHAR(50) NOT NULL,
    [nombre_empresa] NVARCHAR(150) NOT NULL,
    [contacto_nombre] NVARCHAR(100) NULL,
    [telefono] NVARCHAR(20) NULL,
    [correo] NVARCHAR(150) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Proveedores] PRIMARY KEY CLUSTERED ([id_proveedor]),
    CONSTRAINT [UQ_Proveedores_cedula] UNIQUE ([cedula_juridica])
);
END';

    EXEC sys.sp_executesql N'-- Categorias_Insumo
IF OBJECT_ID(N''dbo.Categorias_Insumo'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Categorias_Insumo] (
    [id_categoria] INT IDENTITY(1,1) NOT NULL,
    [nombre_categoria] NVARCHAR(100) NOT NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Categorias_Insumo] PRIMARY KEY CLUSTERED ([id_categoria]),
    CONSTRAINT [UQ_Categorias_Insumo_nombre] UNIQUE ([nombre_categoria])
);
END';

    EXEC sys.sp_executesql N'-- Insumos
IF OBJECT_ID(N''dbo.Insumos'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Insumos] (
    [id_insumo] INT IDENTITY(1,1) NOT NULL,
    [id_categoria] INT NOT NULL,
    [nombre_insumo] NVARCHAR(120) NOT NULL,
    [unidad_medida] NVARCHAR(30) NOT NULL,
    [stock_minimo] DECIMAL(10,2) NOT NULL DEFAULT 0,
    [stock_actual] DECIMAL(10,2) NOT NULL DEFAULT 0,
    [costo_unitario] DECIMAL(10,2) NOT NULL DEFAULT 0,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Insumos] PRIMARY KEY CLUSTERED ([id_insumo]),
    CONSTRAINT [FK_Insumos_Categoria] FOREIGN KEY ([id_categoria]) REFERENCES [dbo].[Categorias_Insumo] ([id_categoria])
);
END';

    EXEC sys.sp_executesql N'-- Insumos_Proveedores
IF OBJECT_ID(N''dbo.Insumos_Proveedores'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Insumos_Proveedores] (
    [id_insumo] INT NOT NULL,
    [id_proveedor] INT NOT NULL,
    [fecha_asoc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Insumos_Proveedores] PRIMARY KEY CLUSTERED ([id_insumo], [id_proveedor]),
    CONSTRAINT [FK_Insumos_Proveedores_Insumo] FOREIGN KEY ([id_insumo]) REFERENCES [dbo].[Insumos] ([id_insumo]),
    CONSTRAINT [FK_Insumos_Proveedores_Proveedor] FOREIGN KEY ([id_proveedor]) REFERENCES [dbo].[Proveedores] ([id_proveedor])
);
END';

    EXEC sys.sp_executesql N'-- Categorias_Producto
IF OBJECT_ID(N''dbo.Categorias_Producto'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Categorias_Producto] (
    [id_categoria_prod] INT IDENTITY(1,1) NOT NULL,
    [nombre_categoria] NVARCHAR(100) NOT NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Categorias_Producto] PRIMARY KEY CLUSTERED ([id_categoria_prod]),
    CONSTRAINT [UQ_Categorias_Producto_nombre] UNIQUE ([nombre_categoria])
);
END';

    EXEC sys.sp_executesql N'-- Productos
IF OBJECT_ID(N''dbo.Productos'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Productos] (
    [id_producto] INT IDENTITY(1,1) NOT NULL,
    [id_categoria_prod] INT NOT NULL,
    [nombre_producto] NVARCHAR(120) NOT NULL,
    [descripcion] NVARCHAR(500) NULL,
    [precio_venta] DECIMAL(10,2) NOT NULL,
    [disponible] BIT NOT NULL DEFAULT 1,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Productos] PRIMARY KEY CLUSTERED ([id_producto]),
    CONSTRAINT [FK_Productos_Categoria] FOREIGN KEY ([id_categoria_prod]) REFERENCES [dbo].[Categorias_Producto] ([id_categoria_prod])
);
END';

    EXEC sys.sp_executesql N'-- Receta
IF OBJECT_ID(N''dbo.Receta'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Receta] (
    [id_receta] INT IDENTITY(1,1) NOT NULL,
    [id_producto] INT NOT NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Receta] PRIMARY KEY CLUSTERED ([id_receta]),
    CONSTRAINT [FK_Receta_Producto] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[Productos] ([id_producto])
);
END';

    EXEC sys.sp_executesql N'-- RecetaInsumo
IF OBJECT_ID(N''dbo.RecetaInsumo'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[RecetaInsumo] (
    [id_receta] INT NOT NULL,
    [id_insumo] INT NOT NULL,
    [cantidad_usar] DECIMAL(10,4) NOT NULL,
    CONSTRAINT [PK_RecetaInsumo] PRIMARY KEY CLUSTERED ([id_receta], [id_insumo]),
    CONSTRAINT [FK_RecetaInsumo_Receta] FOREIGN KEY ([id_receta]) REFERENCES [dbo].[Receta] ([id_receta]),
    CONSTRAINT [FK_RecetaInsumo_Insumo] FOREIGN KEY ([id_insumo]) REFERENCES [dbo].[Insumos] ([id_insumo])
);
END';

    EXEC sys.sp_executesql N'-- Cajas
IF OBJECT_ID(N''dbo.Cajas'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Cajas] (
    [id_caja] INT IDENTITY(1,1) NOT NULL,
    [nombre_caja] NVARCHAR(100) NOT NULL,
    [estado_caja] NVARCHAR(20) NOT NULL DEFAULT ''cerrada'',
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Cajas] PRIMARY KEY CLUSTERED ([id_caja])
);
END';

    EXEC sys.sp_executesql N'-- Apertura_Caja
IF OBJECT_ID(N''dbo.Apertura_Caja'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Apertura_Caja] (
    [id_apertura] INT IDENTITY(1,1) NOT NULL,
    [id_caja] INT NOT NULL,
    [id_cajero] INT NOT NULL,
    [monto_inicial] DECIMAL(12,2) NOT NULL,
    [fecha_apertura] DATETIME2 NOT NULL,
    [observaciones] NVARCHAR(300) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Apertura_Caja] PRIMARY KEY CLUSTERED ([id_apertura]),
    CONSTRAINT [FK_Apertura_Caja_Caja] FOREIGN KEY ([id_caja]) REFERENCES [dbo].[Cajas] ([id_caja]),
    CONSTRAINT [FK_Apertura_Caja_Cajero] FOREIGN KEY ([id_cajero]) REFERENCES [dbo].[Empleados] ([id_empleado])
);
END';

    EXEC sys.sp_executesql N'-- Cierres_Caja
IF OBJECT_ID(N''dbo.Cierres_Caja'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Cierres_Caja] (
    [id_cierre] INT IDENTITY(1,1) NOT NULL,
    [id_cajero] INT NOT NULL,
    [id_apertura] INT NOT NULL,
    [fecha_cierre] DATETIME2 NOT NULL,
    [monto_apertura] DECIMAL(12,2) NOT NULL,
    [total_efectivo] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [total_sinpe] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [total_tarjeta] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [total_egresos] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [saldo_esperado] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [saldo_real] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [descuadre] BIT NOT NULL DEFAULT 0,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Cierres_Caja] PRIMARY KEY CLUSTERED ([id_cierre]),
    CONSTRAINT [FK_Cierres_Caja_Apertura] FOREIGN KEY ([id_apertura]) REFERENCES [dbo].[Apertura_Caja] ([id_apertura]),
    CONSTRAINT [FK_Cierres_Caja_Cajero] FOREIGN KEY ([id_cajero]) REFERENCES [dbo].[Empleados] ([id_empleado])
);
END';

    EXEC sys.sp_executesql N'-- Egresos_Caja
IF OBJECT_ID(N''dbo.Egresos_Caja'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Egresos_Caja] (
    [id_egreso] INT IDENTITY(1,1) NOT NULL,
    [id_apertura] INT NOT NULL,
    [id_usuario] INT NOT NULL,
    [categoria_gasto] NVARCHAR(80) NOT NULL,
    [descripcion] NVARCHAR(300) NOT NULL,
    [monto] DECIMAL(12,2) NOT NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Egresos_Caja] PRIMARY KEY CLUSTERED ([id_egreso]),
    CONSTRAINT [FK_Egresos_Caja_Apertura] FOREIGN KEY ([id_apertura]) REFERENCES [dbo].[Apertura_Caja] ([id_apertura]),
    CONSTRAINT [FK_Egresos_Caja_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END';

    EXEC sys.sp_executesql N'-- Pedidos
IF OBJECT_ID(N''dbo.Pedidos'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Pedidos] (
    [id_pedido] INT IDENTITY(1,1) NOT NULL,
    [id_mesa] INT NULL,
    [id_empleado] INT NOT NULL,
    [tipo_servicio] NVARCHAR(30) NOT NULL DEFAULT ''mesa'',
    [cantidad_comensales] TINYINT NOT NULL DEFAULT 1,
    [estado_pedido] NVARCHAR(20) NOT NULL DEFAULT ''abierto'',
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [observaciones] NVARCHAR(500) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    [fecha_hora_entrega] DATETIME2 NULL,
    [fecha_hora_finalizacion] DATETIME2 NULL,
    CONSTRAINT [PK_Pedidos] PRIMARY KEY CLUSTERED ([id_pedido]),
    CONSTRAINT [FK_Pedidos_Mesa] FOREIGN KEY ([id_mesa]) REFERENCES [dbo].[Mesas] ([id_mesa]),
    CONSTRAINT [FK_Pedidos_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado])
);
END';

    EXEC sys.sp_executesql N'-- Detalle_Pedido
IF OBJECT_ID(N''dbo.Detalle_Pedido'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Detalle_Pedido] (
    [id_detalle] INT IDENTITY(1,1) NOT NULL,
    [id_pedido] INT NOT NULL,
    [id_producto] INT NOT NULL,
    [cantidad] INT NOT NULL DEFAULT 1,
    [precio_unitario] DECIMAL(10,2) NOT NULL,
    [observaciones_item] NVARCHAR(300) NULL,
    [estado_item] NVARCHAR(20) NOT NULL DEFAULT ''pendiente'',
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Detalle_Pedido] PRIMARY KEY CLUSTERED ([id_detalle]),
    CONSTRAINT [FK_Detalle_Pedido_Pedido] FOREIGN KEY ([id_pedido]) REFERENCES [dbo].[Pedidos] ([id_pedido]),
    CONSTRAINT [FK_Detalle_Pedido_Producto] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[Productos] ([id_producto])
);
END';

    EXEC sys.sp_executesql N'-- Historial_Estados_Pedido
IF OBJECT_ID(N''dbo.Historial_Estados_Pedido'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Historial_Estados_Pedido] (
    [id_historial] INT IDENTITY(1,1) NOT NULL,
    [id_pedido] INT NOT NULL,
    [estado_anterior] NVARCHAR(30) NULL,
    [estado_nuevo] NVARCHAR(30) NOT NULL,
    [usuario_responsable] NVARCHAR(80) NULL,
    [fecha_hora_cambio] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [detalle] NVARCHAR(300) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Historial_Estados_Pedido] PRIMARY KEY CLUSTERED ([id_historial])
);
END';

    EXEC sys.sp_executesql N'-- Subcuentas_Pedido
IF OBJECT_ID(N''dbo.Subcuentas_Pedido'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Subcuentas_Pedido] (
    [id_subcuenta] INT IDENTITY(1,1) NOT NULL,
    [id_pedido] INT NOT NULL,
    [nombre_subcuenta] NVARCHAR(50) NOT NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    [fecha_operacion] DATETIME2 NULL,
    CONSTRAINT [PK_Subcuentas_Pedido] PRIMARY KEY CLUSTERED ([id_subcuenta]),
    CONSTRAINT [FK_Subcuentas_Pedido_Pedido] FOREIGN KEY ([id_pedido]) REFERENCES [dbo].[Pedidos] ([id_pedido])
);
END';

    EXEC sys.sp_executesql N'-- Ventas
IF OBJECT_ID(N''dbo.Ventas'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Ventas] (
    [id_venta] INT IDENTITY(1,1) NOT NULL,
    [id_pedido] INT NULL,
    [id_subcuenta] INT NULL,
    [id_empleado] INT NOT NULL,
    [id_apertura] INT NULL,
    [tipo_venta] NVARCHAR(20) NOT NULL DEFAULT ''normal'',
    [total_cobrado] DECIMAL(12,2) NOT NULL,
    [monto_recibido] DECIMAL(12,2) NOT NULL,
    [vuelto] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [metodo_pago] NVARCHAR(30) NOT NULL,
    [estado_venta] NVARCHAR(20) NOT NULL DEFAULT ''completada'',
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Ventas] PRIMARY KEY CLUSTERED ([id_venta]),
    CONSTRAINT [FK_Ventas_Pedido] FOREIGN KEY ([id_pedido]) REFERENCES [dbo].[Pedidos] ([id_pedido]),
    CONSTRAINT [FK_Ventas_Apertura] FOREIGN KEY ([id_apertura]) REFERENCES [dbo].[Apertura_Caja] ([id_apertura])
);
END';

    EXEC sys.sp_executesql N'-- Detalle_Venta
IF OBJECT_ID(N''dbo.Detalle_Venta'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Detalle_Venta] (
    [id_detalle_venta] INT IDENTITY(1,1) NOT NULL,
    [id_venta] INT NOT NULL,
    [id_producto] INT NOT NULL,
    [cantidad] INT NOT NULL,
    [precio_unitario] DECIMAL(10,2) NOT NULL,
    [subtotal_item] DECIMAL(12,2) NOT NULL,
    [observaciones_item] NVARCHAR(300) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Detalle_Venta] PRIMARY KEY CLUSTERED ([id_detalle_venta]),
    CONSTRAINT [FK_Detalle_Venta_Venta] FOREIGN KEY ([id_venta]) REFERENCES [dbo].[Ventas] ([id_venta]),
    CONSTRAINT [FK_Detalle_Venta_Producto] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[Productos] ([id_producto])
);
END';

    EXEC sys.sp_executesql N'-- Notas_Credito
IF OBJECT_ID(N''dbo.Notas_Credito'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Notas_Credito] (
    [id_nota_credito] INT IDENTITY(1,1) NOT NULL,
    [id_venta] INT NOT NULL,
    [id_usuario] INT NOT NULL,
    [motivo] NVARCHAR(300) NOT NULL,
    [monto] DECIMAL(12,2) NOT NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Notas_Credito] PRIMARY KEY CLUSTERED ([id_nota_credito]),
    CONSTRAINT [FK_Notas_Credito_Venta] FOREIGN KEY ([id_venta]) REFERENCES [dbo].[Ventas] ([id_venta]),
    CONSTRAINT [FK_Notas_Credito_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END';

    EXEC sys.sp_executesql N'-- Turnos_Trabajo
IF OBJECT_ID(N''dbo.Turnos_Trabajo'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Turnos_Trabajo] (
    [id_turno] INT IDENTITY(1,1) NOT NULL,
    [id_empleado] INT NOT NULL,
    [fecha_turno] DATE NOT NULL,
    [hora_inicio] TIME NOT NULL,
    [hora_fin] TIME NOT NULL,
    [descripcion] NVARCHAR(200) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Turnos_Trabajo] PRIMARY KEY CLUSTERED ([id_turno]),
    CONSTRAINT [FK_Turnos_Trabajo_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado])
);
END';

    EXEC sys.sp_executesql N'-- Asistencia
IF OBJECT_ID(N''dbo.Asistencia'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Asistencia] (
    [id_asistencia] INT IDENTITY(1,1) NOT NULL,
    [id_empleado] INT NOT NULL,
    [id_turno] INT NULL,
    [fecha_hora_entrada] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [fecha_hora_salida] DATETIME2 NULL,
    [observaciones] NVARCHAR(300) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Asistencia] PRIMARY KEY CLUSTERED ([id_asistencia]),
    CONSTRAINT [FK_Asistencia_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado]),
    CONSTRAINT [FK_Asistencia_Turno] FOREIGN KEY ([id_turno]) REFERENCES [dbo].[Turnos_Trabajo] ([id_turno])
);
END';

    EXEC sys.sp_executesql N'-- Horas_Extra
IF OBJECT_ID(N''dbo.Horas_Extra'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Horas_Extra] (
    [id_hora_extra] INT IDENTITY(1,1) NOT NULL,
    [id_asistencia] INT NOT NULL,
    [cantidad_horas] DECIMAL(6,2) NOT NULL,
    [factor_pago] DECIMAL(4,2) NOT NULL DEFAULT 1.50,
    [monto_calculado] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [fecha_registro] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [motivo_ajuste] NVARCHAR(200) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Horas_Extra] PRIMARY KEY CLUSTERED ([id_hora_extra]),
    CONSTRAINT [FK_Horas_Extra_Asistencia] FOREIGN KEY ([id_asistencia]) REFERENCES [dbo].[Asistencia] ([id_asistencia])
);
END';

    EXEC sys.sp_executesql N'-- Vacaciones
IF OBJECT_ID(N''dbo.Vacaciones'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Vacaciones] (
    [id_vacacion] INT IDENTITY(1,1) NOT NULL,
    [id_empleado] INT NOT NULL,
    [id_aprobador] INT NULL,
    [fecha_inicio] DATE NOT NULL,
    [fecha_fin] DATE NOT NULL,
    [dias_solicitados] DECIMAL(5,2) NOT NULL,
    [estado_solicitud] NVARCHAR(20) NOT NULL DEFAULT ''pendiente'',
    [motivo_rechazo] NVARCHAR(250) NULL,
    [fecha_solicitud] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Vacaciones] PRIMARY KEY CLUSTERED ([id_vacacion]),
    CONSTRAINT [FK_Vacaciones_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado])
);
END';

    EXEC sys.sp_executesql N'-- nomina_mensual
IF OBJECT_ID(N''dbo.nomina_mensual'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[nomina_mensual] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [empleado_id] INT NOT NULL,
    [mes] INT NOT NULL,
    [anio] INT NOT NULL,
    [horas_trabajadas] DECIMAL(8,2) NOT NULL DEFAULT 0,
    [horas_extra] DECIMAL(8,2) NOT NULL DEFAULT 0,
    [vacaciones_pagadas] DECIMAL(8,2) NOT NULL DEFAULT 0,
    [horas_por_dia_vacacion] DECIMAL(8,2) NOT NULL DEFAULT 8,
    [monto_vacaciones] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [dias_trabajados] INT NOT NULL DEFAULT 0,
    [dias_ausentes] INT NOT NULL DEFAULT 0,
    [valor_hora] DECIMAL(10,2) NOT NULL DEFAULT 0,
    [salario_base] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [monto_horas_extra] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [bonificaciones] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [salario_bruto] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [estado] NVARCHAR(20) NOT NULL DEFAULT ''pendiente'',
    [fecha_cierre] DATETIME2 NULL,
    [observaciones] NVARCHAR(500) NULL,
    [creado_por] INT NOT NULL,
    [created_at] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [updated_at] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_nomina_mensual] PRIMARY KEY CLUSTERED ([id]),
    CONSTRAINT [FK_nomina_mensual_Empleado] FOREIGN KEY ([empleado_id]) REFERENCES [dbo].[Empleados] ([id_empleado])
);
END';

    EXEC sys.sp_executesql N'-- Bitácoras y Auditoría
IF OBJECT_ID(N''dbo.Bitacora_Usuarios_RRHH'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Usuarios_RRHH] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [tabla_afectada] NVARCHAR(80) NOT NULL,
    [id_registro_afectado] INT NOT NULL,
    [accion] NVARCHAR(30) NOT NULL,
    [valor_anterior] NVARCHAR(MAX) NULL,
    [valor_nuevo] NVARCHAR(MAX) NULL,
    [detalle] NVARCHAR(500) NULL,
    [ip_origen] NVARCHAR(50) NULL,
    [dispositivo] NVARCHAR(500) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Bitacora_Usuarios_RRHH] PRIMARY KEY CLUSTERED ([id_registro]),
    CONSTRAINT [FK_Bitacora_RRHH_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
ELSE
BEGIN
    IF COL_LENGTH(''dbo.Bitacora_Usuarios_RRHH'', ''ip_origen'') IS NULL
        ALTER TABLE dbo.Bitacora_Usuarios_RRHH ADD ip_origen NVARCHAR(50) NULL;
    IF COL_LENGTH(''dbo.Bitacora_Usuarios_RRHH'', ''dispositivo'') IS NULL
        ALTER TABLE dbo.Bitacora_Usuarios_RRHH ADD dispositivo NVARCHAR(250) NULL;
END';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Bitacora_Pedidos'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Pedidos] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [id_pedido] INT NOT NULL,
    [accion] NVARCHAR(30) NOT NULL,
    [estado_anterior] NVARCHAR(30) NULL,
    [estado_nuevo] NVARCHAR(30) NOT NULL,
    [detalle] NVARCHAR(500) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Bitacora_Pedidos] PRIMARY KEY CLUSTERED ([id_registro])
);
END';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Bitacora_Inventario'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Inventario] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [accion] NVARCHAR(30) NOT NULL,
    [valor_anterior] NVARCHAR(MAX) NULL,
    [valor_nuevo] NVARCHAR(MAX) NULL,
    [detalle] NVARCHAR(500) NULL,
    [ip_origen] NVARCHAR(50) NULL,
    [dispositivo] NVARCHAR(500) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Bitacora_Inventario] PRIMARY KEY CLUSTERED ([id_registro]),
    CONSTRAINT [FK_Bitacora_Inventario_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
ELSE
BEGIN
    IF COL_LENGTH(''dbo.Bitacora_Inventario'', ''ip_origen'') IS NULL
        ALTER TABLE dbo.Bitacora_Inventario ADD ip_origen NVARCHAR(50) NULL;
    IF COL_LENGTH(''dbo.Bitacora_Inventario'', ''dispositivo'') IS NULL
        ALTER TABLE dbo.Bitacora_Inventario ADD dispositivo NVARCHAR(250) NULL;
END';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Bitacora_Acceso'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Acceso] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [accion] NVARCHAR(50) NOT NULL,
    [valor_anterior] NVARCHAR(MAX) NULL,
    [valor_nuevo] NVARCHAR(MAX) NULL,
    [detalle] NVARCHAR(500) NULL,
    [ip_origen] NVARCHAR(50) NULL,
    [dispositivo] NVARCHAR(500) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Bitacora_Acceso] PRIMARY KEY CLUSTERED ([id_registro]),
    CONSTRAINT [FK_Bitacora_Acceso_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
ELSE
BEGIN
    IF COL_LENGTH(''dbo.Bitacora_Acceso'', ''ip_origen'') IS NULL
        ALTER TABLE dbo.Bitacora_Acceso ADD ip_origen NVARCHAR(50) NULL;
    IF COL_LENGTH(''dbo.Bitacora_Acceso'', ''dispositivo'') IS NULL
        ALTER TABLE dbo.Bitacora_Acceso ADD dispositivo NVARCHAR(250) NULL;
END';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Bitacora_PDV'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_PDV] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_caja] INT NULL,
    [id_usuario_cajero] INT NULL,
    [accion_operativa] NVARCHAR(50) NOT NULL,
    [id_venta] INT NULL,
    [detalle] NVARCHAR(500) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Bitacora_PDV] PRIMARY KEY CLUSTERED ([id_registro])
);
END';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Bitacora_Financiera'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Financiera] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [tabla_afectada] NVARCHAR(80) NOT NULL,
    [id_registro_afectado] INT NOT NULL,
    [accion] NVARCHAR(30) NOT NULL,
    [valor_anterior] NVARCHAR(MAX) NULL,
    [valor_nuevo] NVARCHAR(MAX) NULL,
    [detalle] NVARCHAR(500) NULL,
    [ip_origen] NVARCHAR(50) NULL,
    [dispositivo] NVARCHAR(500) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Bitacora_Financiera] PRIMARY KEY CLUSTERED ([id_registro]),
    CONSTRAINT [FK_Bitacora_Financiera_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
ELSE
BEGIN
    IF COL_LENGTH(''dbo.Bitacora_Financiera'', ''ip_origen'') IS NULL
        ALTER TABLE dbo.Bitacora_Financiera ADD ip_origen NVARCHAR(50) NULL;
    IF COL_LENGTH(''dbo.Bitacora_Financiera'', ''dispositivo'') IS NULL
        ALTER TABLE dbo.Bitacora_Financiera ADD dispositivo NVARCHAR(250) NULL;
END';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Bitacora_Reportes'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Reportes] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [accion] NVARCHAR(30) NOT NULL,
    [valor_anterior] NVARCHAR(MAX) NULL,
    [valor_nuevo] NVARCHAR(MAX) NULL,
    [detalle] NVARCHAR(500) NULL,
    [ip_origen] NVARCHAR(50) NULL,
    [dispositivo] NVARCHAR(500) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Bitacora_Reportes] PRIMARY KEY CLUSTERED ([id_registro]),
    CONSTRAINT [FK_Bitacora_Reportes_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
ELSE
BEGIN
    IF COL_LENGTH(''dbo.Bitacora_Reportes'', ''ip_origen'') IS NULL
        ALTER TABLE dbo.Bitacora_Reportes ADD ip_origen NVARCHAR(50) NULL;
    IF COL_LENGTH(''dbo.Bitacora_Reportes'', ''dispositivo'') IS NULL
        ALTER TABLE dbo.Bitacora_Reportes ADD dispositivo NVARCHAR(250) NULL;
END';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Reportes_Generados'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Reportes_Generados] (
    [id_reporte] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [tipo_reporte] NVARCHAR(50) NOT NULL,
    [parametros] NVARCHAR(MAX) NULL,
    [formato_salida] NVARCHAR(20) NOT NULL,
    [fecha_generacion] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Reportes_Generados] PRIMARY KEY CLUSTERED ([id_reporte]),
    CONSTRAINT [FK_Reportes_Generados_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Cierres_Periodo'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Cierres_Periodo] (
    [id_cierre_periodo] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [tipo_periodo] NVARCHAR(20) NOT NULL,
    [mes] INT NULL,
    [anio] INT NOT NULL,
    [fecha_cierre] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [total_ingresos] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [total_egresos] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [total_notas_credito] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [saldo_final] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Cierres_Periodo] PRIMARY KEY CLUSTERED ([id_cierre_periodo]),
    CONSTRAINT [FK_Cierres_Periodo_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Sesiones'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Sesiones] (
    [id_sesion] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [estado_sesion] NVARCHAR(20) NOT NULL DEFAULT ''ACTIVA'',
    [fecha_hora_inicio] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [fecha_hora_ultima_act] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [fecha_hora_cierre] DATETIME2 NULL,
    [dispositivo_acceso] NVARCHAR(500) NULL,
    [direccion_ip] NVARCHAR(50) NULL,
    [motivo_cierre] NVARCHAR(200) NULL,
    CONSTRAINT [PK_Sesiones] PRIMARY KEY CLUSTERED ([id_sesion]),
    CONSTRAINT [FK_Sesiones_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.PasswordHistorial'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[PasswordHistorial] (
    [id_historial] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [password_hash] NVARCHAR(255) NOT NULL,
    [fecha_cambio] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [metodo_cambio] NVARCHAR(50) NULL,
    [dispositivo] NVARCHAR(500) NULL,
    [direccion_ip] NVARCHAR(50) NULL,
    CONSTRAINT [PK_PasswordHistorial] PRIMARY KEY CLUSTERED ([id_historial]),
    CONSTRAINT [FK_PasswordHistorial_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Envios_PDV'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Envios_PDV] (
    [id_envio]           INT            IDENTITY (1, 1) NOT NULL,
    [id_venta]           INT            NOT NULL,
    [id_usuario]         INT            NOT NULL,
    [destino]            NVARCHAR (20)  NOT NULL,
    [prioridad]          NVARCHAR (10)  NOT NULL,
    [estado_envio]       NVARCHAR (12)  NOT NULL,
    [codigo_confirmacion] NVARCHAR (50) NULL,
    [observaciones]      NVARCHAR (500) NULL,
    [motivo_reenvio]     NVARCHAR (300) NULL,
    [fecha_hora_envio]   DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_Envios_PDV] PRIMARY KEY CLUSTERED ([id_envio] ASC),
    CONSTRAINT [CK_EnvPDV_destino] CHECK ([destino] IN (''Caja'', ''Bar'', ''Cocina'')),
    CONSTRAINT [CK_EnvPDV_prioridad] CHECK ([prioridad] IN (''Urgente'', ''Normal'')),
    CONSTRAINT [CK_EnvPDV_estado] CHECK ([estado_envio] IN (''Enviado'', ''Fallido'', ''Pendiente'', ''Reenviado''))
);
END';

    EXEC sys.sp_executesql N'IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N''dbo.Envios_PDV'') AND name=N''IX_EnvPDV_venta'')
CREATE NONCLUSTERED INDEX [IX_EnvPDV_venta]
    ON [dbo].[Envios_PDV]([id_venta] ASC);';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Subcuentas_Venta'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Subcuentas_Venta] (
    [id_subcuenta]    INT            IDENTITY (1, 1) NOT NULL,
    [id_venta]        INT            NOT NULL,
    [nombre_subcuenta] NVARCHAR (100) NOT NULL,
    [subtotal]        DECIMAL (12, 2) NOT NULL,
    [pagada]          BIT            NOT NULL,
    [fecha_operacion] DATETIME2 (7)  NOT NULL,
    [estado]          BIT            NOT NULL,
    CONSTRAINT [PK_Subcuentas_Venta] PRIMARY KEY CLUSTERED ([id_subcuenta] ASC)
);
END';

    EXEC sys.sp_executesql N'IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N''dbo.Subcuentas_Venta'') AND name=N''IX_SCV_venta'')
CREATE NONCLUSTERED INDEX [IX_SCV_venta]
    ON [dbo].[Subcuentas_Venta]([id_venta] ASC);';

    EXEC sys.sp_executesql N'IF OBJECT_ID(N''dbo.Subcuenta_Detalle_Venta'', N''U'') IS NULL
BEGIN
CREATE TABLE [dbo].[Subcuenta_Detalle_Venta] (
    [id_subcuenta_detalle] INT            IDENTITY (1, 1) NOT NULL,
    [id_subcuenta]  INT            NOT NULL,
    [id_detalle_venta] INT           NOT NULL,
    [cantidad]      INT            NOT NULL,
    [subtotal]      DECIMAL (12, 2) NOT NULL,
    [fecha_operacion] DATETIME2 (7) NOT NULL,
    [estado]        BIT            NOT NULL,
    CONSTRAINT [PK_Subcuenta_Detalle_Venta] PRIMARY KEY CLUSTERED ([id_subcuenta_detalle] ASC)
);
END';

    EXEC sys.sp_executesql N'IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N''dbo.Subcuenta_Detalle_Venta'') AND name=N''IX_SCDV_subcuenta'')
CREATE NONCLUSTERED INDEX [IX_SCDV_subcuenta]
    ON [dbo].[Subcuenta_Detalle_Venta]([id_subcuenta] ASC);';

    EXEC sys.sp_executesql N'-- Compatibilidad no destructiva para instalaciones anteriores del mismo esquema.
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N''dbo.Ventas'') AND name=N''id_pedido'' AND is_nullable=0)
    ALTER TABLE dbo.Ventas ALTER COLUMN id_pedido INT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N''dbo.Detalle_Pedido'') AND name=N''cantidad'' AND system_type_id<>TYPE_ID(N''int''))
    THROW 51001, ''Detalle_Pedido.cantidad debe ser INT. Revise sus datos antes de migrar ese tipo.'', 1;
';
    COMMIT TRANSACTION;
    SELECT DB_NAME() AS base_de_datos, COUNT(*) AS tablas_dbo FROM sys.tables WHERE schema_id=SCHEMA_ID(N'dbo');
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
