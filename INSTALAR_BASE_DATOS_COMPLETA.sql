-- ==========================================================================================
-- SCRIPT MAESTRO DE INSTALACIÓN Y CARGA INICIAL — RESTAURANTE EL COLIBRÍ
-- Base de Datos: COLIBRI
-- ==========================================================================================

USE master;
GO

IF DB_ID('COLIBRI') IS NULL
BEGIN
    CREATE DATABASE [COLIBRI] COLLATE Modern_Spanish_CI_AI;
    PRINT 'Base de datos COLIBRI creada.';
END
GO

USE [COLIBRI];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- 1. LIMPIEZA DE CONSTRAINTS RESTRICTIVAS Y TRIGGERS QUE PUEDAN CAUSAR BLOQUEOS
IF EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_BitRR_accion')
    ALTER TABLE dbo.Bitacora_Usuarios_RRHH DROP CONSTRAINT CK_BitRR_accion;
GO

-- 2. CREACIÓN DE TABLAS (SI NO EXISTEN)

-- Roles
IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Roles] (
    [id_rol] INT IDENTITY(1,1) NOT NULL,
    [nombre_rol] NVARCHAR(60) NOT NULL,
    [descripcion] NVARCHAR(255) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([id_rol]),
    CONSTRAINT [UQ_Roles_nombre] UNIQUE ([nombre_rol])
);
END
GO

-- Usuarios
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
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
END
GO

-- Empleados
IF OBJECT_ID(N'dbo.Empleados', N'U') IS NULL
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
END
GO

-- Mesas
IF OBJECT_ID(N'dbo.Mesas', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Mesas] (
    [id_mesa] INT IDENTITY(1,1) NOT NULL,
    [numero_mesa] NVARCHAR(10) NOT NULL,
    [capacidad] TINYINT NOT NULL DEFAULT 4,
    [estado_mesa] NVARCHAR(20) NOT NULL DEFAULT 'disponible',
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Mesas] PRIMARY KEY CLUSTERED ([id_mesa]),
    CONSTRAINT [UQ_Mesas_numero] UNIQUE ([numero_mesa])
);
END
GO

-- Proveedores
IF OBJECT_ID(N'dbo.Proveedores', N'U') IS NULL
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
END
GO

-- Categorias_Insumo
IF OBJECT_ID(N'dbo.Categorias_Insumo', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Categorias_Insumo] (
    [id_categoria] INT IDENTITY(1,1) NOT NULL,
    [nombre_categoria] NVARCHAR(100) NOT NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Categorias_Insumo] PRIMARY KEY CLUSTERED ([id_categoria]),
    CONSTRAINT [UQ_Categorias_Insumo_nombre] UNIQUE ([nombre_categoria])
);
END
GO

-- Insumos
IF OBJECT_ID(N'dbo.Insumos', N'U') IS NULL
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
END
GO

-- Insumos_Proveedores
IF OBJECT_ID(N'dbo.Insumos_Proveedores', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Insumos_Proveedores] (
    [id_insumo] INT NOT NULL,
    [id_proveedor] INT NOT NULL,
    [fecha_asoc] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Insumos_Proveedores] PRIMARY KEY CLUSTERED ([id_insumo], [id_proveedor]),
    CONSTRAINT [FK_Insumos_Proveedores_Insumo] FOREIGN KEY ([id_insumo]) REFERENCES [dbo].[Insumos] ([id_insumo]),
    CONSTRAINT [FK_Insumos_Proveedores_Proveedor] FOREIGN KEY ([id_proveedor]) REFERENCES [dbo].[Proveedores] ([id_proveedor])
);
END
GO

-- Categorias_Producto
IF OBJECT_ID(N'dbo.Categorias_Producto', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Categorias_Producto] (
    [id_categoria_prod] INT IDENTITY(1,1) NOT NULL,
    [nombre_categoria] NVARCHAR(100) NOT NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Categorias_Producto] PRIMARY KEY CLUSTERED ([id_categoria_prod]),
    CONSTRAINT [UQ_Categorias_Producto_nombre] UNIQUE ([nombre_categoria])
);
END
GO

-- Productos
IF OBJECT_ID(N'dbo.Productos', N'U') IS NULL
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
END
GO

-- Receta
IF OBJECT_ID(N'dbo.Receta', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Receta] (
    [id_receta] INT IDENTITY(1,1) NOT NULL,
    [id_producto] INT NOT NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Receta] PRIMARY KEY CLUSTERED ([id_receta]),
    CONSTRAINT [FK_Receta_Producto] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[Productos] ([id_producto])
);
END
GO

-- RecetaInsumo
IF OBJECT_ID(N'dbo.RecetaInsumo', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[RecetaInsumo] (
    [id_receta] INT NOT NULL,
    [id_insumo] INT NOT NULL,
    [cantidad_usar] DECIMAL(10,4) NOT NULL,
    CONSTRAINT [PK_RecetaInsumo] PRIMARY KEY CLUSTERED ([id_receta], [id_insumo]),
    CONSTRAINT [FK_RecetaInsumo_Receta] FOREIGN KEY ([id_receta]) REFERENCES [dbo].[Receta] ([id_receta]),
    CONSTRAINT [FK_RecetaInsumo_Insumo] FOREIGN KEY ([id_insumo]) REFERENCES [dbo].[Insumos] ([id_insumo])
);
END
GO

-- Cajas
IF OBJECT_ID(N'dbo.Cajas', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Cajas] (
    [id_caja] INT IDENTITY(1,1) NOT NULL,
    [nombre_caja] NVARCHAR(100) NOT NULL,
    [estado_caja] NVARCHAR(20) NOT NULL DEFAULT 'cerrada',
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Cajas] PRIMARY KEY CLUSTERED ([id_caja])
);
END
GO

-- Apertura_Caja
IF OBJECT_ID(N'dbo.Apertura_Caja', N'U') IS NULL
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
END
GO

-- Cierres_Caja
IF OBJECT_ID(N'dbo.Cierres_Caja', N'U') IS NULL
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
END
GO

-- Egresos_Caja
IF OBJECT_ID(N'dbo.Egresos_Caja', N'U') IS NULL
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
END
GO

-- Pedidos
IF OBJECT_ID(N'dbo.Pedidos', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Pedidos] (
    [id_pedido] INT IDENTITY(1,1) NOT NULL,
    [id_mesa] INT NULL,
    [id_empleado] INT NOT NULL,
    [tipo_servicio] NVARCHAR(30) NOT NULL DEFAULT 'en_mesa',
    [cantidad_comensales] TINYINT NOT NULL DEFAULT 1,
    [estado_pedido] NVARCHAR(20) NOT NULL DEFAULT 'abierto',
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [observaciones] NVARCHAR(500) NULL,
    [estado] BIT NOT NULL DEFAULT 1,
    [fecha_hora_entrega] DATETIME2 NULL,
    [fecha_hora_finalizacion] DATETIME2 NULL,
    CONSTRAINT [PK_Pedidos] PRIMARY KEY CLUSTERED ([id_pedido]),
    CONSTRAINT [FK_Pedidos_Mesa] FOREIGN KEY ([id_mesa]) REFERENCES [dbo].[Mesas] ([id_mesa]),
    CONSTRAINT [FK_Pedidos_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado])
);
END
GO

-- Detalle_Pedido
IF OBJECT_ID(N'dbo.Detalle_Pedido', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Detalle_Pedido] (
    [id_detalle] INT IDENTITY(1,1) NOT NULL,
    [id_pedido] INT NOT NULL,
    [id_producto] INT NOT NULL,
    [cantidad] INT NOT NULL DEFAULT 1,
    [precio_unitario] DECIMAL(10,2) NOT NULL,
    [observaciones_item] NVARCHAR(300) NULL,
    [estado_item] NVARCHAR(20) NOT NULL DEFAULT 'pendiente',
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Detalle_Pedido] PRIMARY KEY CLUSTERED ([id_detalle]),
    CONSTRAINT [FK_Detalle_Pedido_Pedido] FOREIGN KEY ([id_pedido]) REFERENCES [dbo].[Pedidos] ([id_pedido]),
    CONSTRAINT [FK_Detalle_Pedido_Producto] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[Productos] ([id_producto])
);
END
GO

-- Historial_Estados_Pedido
IF OBJECT_ID(N'dbo.Historial_Estados_Pedido', N'U') IS NULL
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
END
GO

-- Subcuentas_Pedido
IF OBJECT_ID(N'dbo.Subcuentas_Pedido', N'U') IS NULL
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
END
GO

-- Ventas
IF OBJECT_ID(N'dbo.Ventas', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Ventas] (
    [id_venta] INT IDENTITY(1,1) NOT NULL,
    [id_pedido] INT NOT NULL,
    [id_subcuenta] INT NULL,
    [id_empleado] INT NOT NULL,
    [id_apertura] INT NULL,
    [tipo_venta] NVARCHAR(20) NOT NULL DEFAULT 'normal',
    [total_cobrado] DECIMAL(12,2) NOT NULL,
    [monto_recibido] DECIMAL(12,2) NOT NULL,
    [vuelto] DECIMAL(12,2) NOT NULL DEFAULT 0,
    [metodo_pago] NVARCHAR(30) NOT NULL,
    [estado_venta] NVARCHAR(20) NOT NULL DEFAULT 'completada',
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Ventas] PRIMARY KEY CLUSTERED ([id_venta]),
    CONSTRAINT [FK_Ventas_Pedido] FOREIGN KEY ([id_pedido]) REFERENCES [dbo].[Pedidos] ([id_pedido]),
    CONSTRAINT [FK_Ventas_Apertura] FOREIGN KEY ([id_apertura]) REFERENCES [dbo].[Apertura_Caja] ([id_apertura])
);
END
GO

-- Detalle_Venta
IF OBJECT_ID(N'dbo.Detalle_Venta', N'U') IS NULL
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
END
GO

-- Notas_Credito
IF OBJECT_ID(N'dbo.Notas_Credito', N'U') IS NULL
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
END
GO

-- Turnos_Trabajo
IF OBJECT_ID(N'dbo.Turnos_Trabajo', N'U') IS NULL
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
END
GO

-- Asistencia
IF OBJECT_ID(N'dbo.Asistencia', N'U') IS NULL
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
END
GO

-- Horas_Extra
IF OBJECT_ID(N'dbo.Horas_Extra', N'U') IS NULL
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
END
GO

-- Vacaciones
IF OBJECT_ID(N'dbo.Vacaciones', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Vacaciones] (
    [id_vacacion] INT IDENTITY(1,1) NOT NULL,
    [id_empleado] INT NOT NULL,
    [id_aprobador] INT NULL,
    [fecha_inicio] DATE NOT NULL,
    [fecha_fin] DATE NOT NULL,
    [dias_solicitados] DECIMAL(5,2) NOT NULL,
    [estado_solicitud] NVARCHAR(20) NOT NULL DEFAULT 'pendiente',
    [motivo_rechazo] NVARCHAR(250) NULL,
    [fecha_solicitud] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [estado] BIT NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Vacaciones] PRIMARY KEY CLUSTERED ([id_vacacion]),
    CONSTRAINT [FK_Vacaciones_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado])
);
END
GO

-- nomina_mensual
IF OBJECT_ID(N'dbo.nomina_mensual', N'U') IS NULL
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
    [estado] NVARCHAR(20) NOT NULL DEFAULT 'pendiente',
    [fecha_cierre] DATETIME2 NULL,
    [observaciones] NVARCHAR(500) NULL,
    [creado_por] INT NOT NULL,
    [created_at] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [updated_at] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_nomina_mensual] PRIMARY KEY CLUSTERED ([id]),
    CONSTRAINT [FK_nomina_mensual_Empleado] FOREIGN KEY ([empleado_id]) REFERENCES [dbo].[Empleados] ([id_empleado])
);
END
GO

-- Bitácoras y Auditoría
IF OBJECT_ID(N'dbo.Bitacora_Usuarios_RRHH', N'U') IS NULL
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
    [dispositivo] NVARCHAR(250) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Bitacora_Usuarios_RRHH] PRIMARY KEY CLUSTERED ([id_registro]),
    CONSTRAINT [FK_Bitacora_RRHH_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Bitacora_Usuarios_RRHH', 'ip_origen') IS NULL
        ALTER TABLE dbo.Bitacora_Usuarios_RRHH ADD ip_origen NVARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Bitacora_Usuarios_RRHH', 'dispositivo') IS NULL
        ALTER TABLE dbo.Bitacora_Usuarios_RRHH ADD dispositivo NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.Bitacora_Pedidos', N'U') IS NULL
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
END
GO

IF OBJECT_ID(N'dbo.Bitacora_Inventario', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Inventario] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [accion] NVARCHAR(30) NOT NULL,
    [valor_anterior] NVARCHAR(MAX) NULL,
    [valor_nuevo] NVARCHAR(MAX) NULL,
    [detalle] NVARCHAR(500) NULL,
    [ip_origen] NVARCHAR(50) NULL,
    [dispositivo] NVARCHAR(250) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Bitacora_Inventario] PRIMARY KEY CLUSTERED ([id_registro]),
    CONSTRAINT [FK_Bitacora_Inventario_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Bitacora_Inventario', 'ip_origen') IS NULL
        ALTER TABLE dbo.Bitacora_Inventario ADD ip_origen NVARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Bitacora_Inventario', 'dispositivo') IS NULL
        ALTER TABLE dbo.Bitacora_Inventario ADD dispositivo NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.Bitacora_Acceso', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Acceso] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [accion] NVARCHAR(50) NOT NULL,
    [valor_anterior] NVARCHAR(MAX) NULL,
    [valor_nuevo] NVARCHAR(MAX) NULL,
    [detalle] NVARCHAR(500) NULL,
    [ip_origen] NVARCHAR(50) NULL,
    [dispositivo] NVARCHAR(250) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Bitacora_Acceso] PRIMARY KEY CLUSTERED ([id_registro]),
    CONSTRAINT [FK_Bitacora_Acceso_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Bitacora_Acceso', 'ip_origen') IS NULL
        ALTER TABLE dbo.Bitacora_Acceso ADD ip_origen NVARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Bitacora_Acceso', 'dispositivo') IS NULL
        ALTER TABLE dbo.Bitacora_Acceso ADD dispositivo NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.Bitacora_PDV', N'U') IS NULL
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
END
GO

IF OBJECT_ID(N'dbo.Bitacora_Financiera', N'U') IS NULL
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
    [dispositivo] NVARCHAR(250) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Bitacora_Financiera] PRIMARY KEY CLUSTERED ([id_registro]),
    CONSTRAINT [FK_Bitacora_Financiera_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Bitacora_Financiera', 'ip_origen') IS NULL
        ALTER TABLE dbo.Bitacora_Financiera ADD ip_origen NVARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Bitacora_Financiera', 'dispositivo') IS NULL
        ALTER TABLE dbo.Bitacora_Financiera ADD dispositivo NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.Bitacora_Reportes', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Reportes] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [accion] NVARCHAR(30) NOT NULL,
    [valor_anterior] NVARCHAR(MAX) NULL,
    [valor_nuevo] NVARCHAR(MAX) NULL,
    [detalle] NVARCHAR(500) NULL,
    [ip_origen] NVARCHAR(50) NULL,
    [dispositivo] NVARCHAR(250) NULL,
    [fecha_hora] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Bitacora_Reportes] PRIMARY KEY CLUSTERED ([id_registro]),
    CONSTRAINT [FK_Bitacora_Reportes_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
ELSE
BEGIN
    IF COL_LENGTH('dbo.Bitacora_Reportes', 'ip_origen') IS NULL
        ALTER TABLE dbo.Bitacora_Reportes ADD ip_origen NVARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Bitacora_Reportes', 'dispositivo') IS NULL
        ALTER TABLE dbo.Bitacora_Reportes ADD dispositivo NVARCHAR(250) NULL;
END
GO

IF OBJECT_ID(N'dbo.Reportes_Generados', N'U') IS NULL
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
END
GO

IF OBJECT_ID(N'dbo.Cierres_Periodo', N'U') IS NULL
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
END
GO

IF OBJECT_ID(N'dbo.Sesiones', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Sesiones] (
    [id_sesion] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [estado_sesion] NVARCHAR(20) NOT NULL DEFAULT 'activa',
    [fecha_hora_inicio] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [fecha_hora_ultima_act] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [fecha_hora_cierre] DATETIME2 NULL,
    [dispositivo_acceso] NVARCHAR(250) NULL,
    [direccion_ip] NVARCHAR(50) NULL,
    [motivo_cierre] NVARCHAR(200) NULL,
    CONSTRAINT [PK_Sesiones] PRIMARY KEY CLUSTERED ([id_sesion]),
    CONSTRAINT [FK_Sesiones_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
GO

IF OBJECT_ID(N'dbo.PasswordHistorial', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PasswordHistorial] (
    [id_historial] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT NOT NULL,
    [password_hash] NVARCHAR(255) NOT NULL,
    [fecha_cambio] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [metodo_cambio] NVARCHAR(50) NULL,
    [dispositivo] NVARCHAR(250) NULL,
    [direccion_ip] NVARCHAR(50) NULL,
    CONSTRAINT [PK_PasswordHistorial] PRIMARY KEY CLUSTERED ([id_historial]),
    CONSTRAINT [FK_PasswordHistorial_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
END
GO

-- ==========================================================================================
-- 3. CARGA DE DATOS INICIALES (SEED REALISTA Y COMPLETO)
-- ==========================================================================================

-- Roles base
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE nombre_rol = N'Administrador')
    INSERT INTO dbo.Roles (nombre_rol, descripcion, estado) VALUES (N'Administrador', N'Acceso total al sistema', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE nombre_rol = N'Cajero')
    INSERT INTO dbo.Roles (nombre_rol, descripcion, estado) VALUES (N'Cajero', N'Gestión de ventas y cierres de caja', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE nombre_rol = N'Mesero')
    INSERT INTO dbo.Roles (nombre_rol, descripcion, estado) VALUES (N'Mesero', N'Toma y gestión de pedidos en sala', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE nombre_rol = N'Cocinero')
    INSERT INTO dbo.Roles (nombre_rol, descripcion, estado) VALUES (N'Cocinero', N'Visualización y actualización de pedidos en cocina', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE nombre_rol = N'Supervisor')
    INSERT INTO dbo.Roles (nombre_rol, descripcion, estado) VALUES (N'Supervisor', N'Reportes y supervisión operativa', 1);
GO

-- Usuarios y Empleados Demo (Contraseña de todos: Password1234.)
-- Hash SHA256 PBKDF2 verificado con PasswordHelper
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = N'admin')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES ((SELECT TOP 1 id_rol FROM dbo.Roles WHERE nombre_rol=N'Administrador'), N'admin', N'admin@colibri.com', N'zV6JT6+5WSY1wJ48nj8yn7EHQ8rBJHPrpu488+KNdYQWKYlMtR9pQUXPj++RyTTo', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, fecha_ingreso, estado)
    VALUES (SCOPE_IDENTITY(), N'0-0000-0000', N'Administrador', N'General', 5000, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = N'cajero_demo')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES ((SELECT TOP 1 id_rol FROM dbo.Roles WHERE nombre_rol=N'Cajero'), N'cajero_demo', N'cajero@colibri.com', N'OhxNO55QZdrhUUuRGuC1fAtu1uCv8mGHK5cbPFUPsDHuIanqGRT0jHqbDjqN5lMV', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, fecha_ingreso, estado)
    VALUES (SCOPE_IDENTITY(), N'2-2222-2222', N'Juan', N'Cajero Demo', 2500, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = N'mesero_demo')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES ((SELECT TOP 1 id_rol FROM dbo.Roles WHERE nombre_rol=N'Mesero'), N'mesero_demo', N'mesero@colibri.com', N'IjeGt57fg2wTSBJrIQqCfnDOBEF6LRp3kQVr32gnLcbagnhl1AL8yA6gpGJ0g6ya', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, fecha_ingreso, estado)
    VALUES (SCOPE_IDENTITY(), N'3-3333-3333', N'María', N'Mesero Demo', 2500, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = N'cocinero_demo')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES ((SELECT TOP 1 id_rol FROM dbo.Roles WHERE nombre_rol=N'Cocinero'), N'cocinero_demo', N'cocinero@colibri.com', N'pEtqmzN3xGL6jzuKRK/lZ9hsBIbwl6phuXykbTmQd54z5XxTvKK+KrGWaq7QK5o5', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, fecha_ingreso, estado)
    VALUES (SCOPE_IDENTITY(), N'4-4444-4444', N'Carlos', N'Cocinero Demo', 3000, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE nombre_usuario = N'supervisor_demo')
BEGIN
    INSERT INTO dbo.Usuarios (id_rol, nombre_usuario, correo, password_hash, cambio_password_requerido, estado, fecha_creacion, fecha_password)
    VALUES ((SELECT TOP 1 id_rol FROM dbo.Roles WHERE nombre_rol=N'Supervisor'), N'supervisor_demo', N'supervisor@colibri.com', N'NiEyjByddIGBvYbwCgpkF/LvtgwiU9XIdqsehf8KFJvpyx9fCfDoyWxeBcLH6Dlg', 0, 1, SYSUTCDATETIME(), SYSUTCDATETIME());

    INSERT INTO dbo.Empleados (id_usuario, cedula, nombre, apellidos, salario_hora, fecha_ingreso, estado)
    VALUES (SCOPE_IDENTITY(), N'5-5555-5555', N'Laura', N'Supervisor Demo', 3500, CAST(SYSUTCDATETIME() AS DATE), 1);
END
GO

-- Mesas (13 mesas disponibles)
IF NOT EXISTS (SELECT 1 FROM dbo.Mesas WHERE numero_mesa = N'1')
BEGIN
    INSERT INTO dbo.Mesas (numero_mesa, capacidad, estado_mesa, estado) VALUES
        (N'1', 4, N'disponible', 1), (N'2', 4, N'disponible', 1),
        (N'3', 4, N'disponible', 1), (N'4', 4, N'disponible', 1),
        (N'5', 4, N'disponible', 1), (N'6', 4, N'disponible', 1),
        (N'7', 4, N'disponible', 1), (N'8', 4, N'disponible', 1),
        (N'9', 4, N'disponible', 1), (N'10', 4, N'disponible', 1),
        (N'11', 4, N'disponible', 1), (N'12', 4, N'disponible', 1),
        (N'13', 6, N'disponible', 1);
END
GO

-- Cajas
IF NOT EXISTS (SELECT 1 FROM dbo.Cajas WHERE nombre_caja = N'Caja Principal Terminal 01')
BEGIN
    INSERT INTO dbo.Cajas (nombre_caja, estado_caja, estado) VALUES
        (N'Caja Principal Terminal 01', N'cerrada', 1),
        (N'Caja Principal Terminal 02', N'cerrada', 1);
END
GO

-- Categorías de Insumo (asegurar categorías esenciales)
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%Carnes%')
    INSERT INTO dbo.Categorias_Insumo (nombre_categoria, estado) VALUES (N'Carnes', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%Granos%')
    INSERT INTO dbo.Categorias_Insumo (nombre_categoria, estado) VALUES (N'Granos y cereales', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%Vegetales%')
    INSERT INTO dbo.Categorias_Insumo (nombre_categoria, estado) VALUES (N'Vegetales', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%cteos%')
    INSERT INTO dbo.Categorias_Insumo (nombre_categoria, estado) VALUES (N'Lácteos', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%Pescad%' OR nombre_categoria LIKE '%Marisc%')
    INSERT INTO dbo.Categorias_Insumo (nombre_categoria, estado) VALUES (N'Pescados y mariscos', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%Caf%')
    INSERT INTO dbo.Categorias_Insumo (nombre_categoria, estado) VALUES (N'Café y té', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%Aceite%')
    INSERT INTO dbo.Categorias_Insumo (nombre_categoria, estado) VALUES (N'Aceites y grasas', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%Bebida%')
    INSERT INTO dbo.Categorias_Insumo (nombre_categoria, estado) VALUES (N'Bebidas', 1);
GO

-- Categorías de Producto
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Producto WHERE nombre_categoria LIKE '%Platos Fuertes%')
    INSERT INTO dbo.Categorias_Producto (nombre_categoria, estado) VALUES (N'Platos Fuertes', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Producto WHERE nombre_categoria LIKE '%Entradas%')
    INSERT INTO dbo.Categorias_Producto (nombre_categoria, estado) VALUES (N'Entradas', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Producto WHERE nombre_categoria LIKE '%Pastas%')
    INSERT INTO dbo.Categorias_Producto (nombre_categoria, estado) VALUES (N'Pastas', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Producto WHERE nombre_categoria LIKE '%Bebidas calientes%')
    INSERT INTO dbo.Categorias_Producto (nombre_categoria, estado) VALUES (N'Bebidas calientes', 1);
IF NOT EXISTS (SELECT 1 FROM dbo.Categorias_Producto WHERE nombre_categoria LIKE '%Postres%')
    INSERT INTO dbo.Categorias_Producto (nombre_categoria, estado) VALUES (N'Postres', 1);
GO

-- Proveedores
IF NOT EXISTS (SELECT 1 FROM dbo.Proveedores WHERE cedula_juridica = N'3-101-123456')
BEGIN
    INSERT INTO dbo.Proveedores (cedula_juridica, nombre_empresa, contacto_nombre, telefono, correo, estado) VALUES
        (N'3-101-123456', N'Distribuidora Alimentos del Valle S.A.', N'Carlos Mora', N'2256-7890', N'carlos@alivalle.com', 1),
        (N'3-102-654321', N'Carnes Premium del Pacífico S.R.L.', N'María Jiménez', N'2288-4567', N'maria@carnespremium.cr', 1),
        (N'3-103-987654', N'Lácteos Monteverde S.A.', N'Ana Rodríguez', N'2267-8901', N'ana@monteverde.com', 1);
END
GO

-- Insumos (con stock inicial para soportar pruebas de recetas y pedidos)
DECLARE @idCatCarnes INT = (SELECT TOP 1 id_categoria FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%Carnes%');
DECLARE @idCatGranos INT = (SELECT TOP 1 id_categoria FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%Granos%');
DECLARE @idCatLacteos INT = (SELECT TOP 1 id_categoria FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%cteos%');
DECLARE @idCatCafe INT = (SELECT TOP 1 id_categoria FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%Caf%');
DECLARE @idCatVeg INT = (SELECT TOP 1 id_categoria FROM dbo.Categorias_Insumo WHERE nombre_categoria LIKE '%Vegetales%');

IF NOT EXISTS (SELECT 1 FROM dbo.Insumos WHERE nombre_insumo = N'Pechuga de pollo fresca')
BEGIN
    INSERT INTO dbo.Insumos (id_categoria, nombre_insumo, unidad_medida, stock_minimo, stock_actual, costo_unitario, estado) VALUES
        (@idCatCarnes, N'Pechuga de pollo fresca', N'kg', 5.00, 50.00, 3200.00, 1),
        (@idCatCarnes, N'Carne de res molida', N'kg', 5.00, 45.00, 3800.00, 1),
        (@idCatGranos, N'Arroz blanco', N'kg', 10.00, 80.00, 950.00, 1),
        (@idCatGranos, N'Frijoles negros', N'kg', 10.00, 60.00, 1200.00, 1),
        (@idCatLacteos, N'Queso mozzarella', N'kg', 4.00, 30.00, 4500.00, 1),
        (@idCatCafe, N'Café arábica gourmet', N'kg', 3.00, 25.00, 6500.00, 1),
        (@idCatVeg, N'Tomate y vegetales surtidos', N'kg', 5.00, 40.00, 1100.00, 1);
END
GO

-- Productos del menú
DECLARE @idCatPlatos INT = (SELECT TOP 1 id_categoria_prod FROM dbo.Categorias_Producto WHERE nombre_categoria LIKE '%Platos Fuertes%');
DECLARE @idCatBebidasCalientes INT = (SELECT TOP 1 id_categoria_prod FROM dbo.Categorias_Producto WHERE nombre_categoria LIKE '%Bebidas calientes%');
DECLARE @idCatEntradas INT = (SELECT TOP 1 id_categoria_prod FROM dbo.Categorias_Producto WHERE nombre_categoria LIKE '%Entradas%');

IF NOT EXISTS (SELECT 1 FROM dbo.Productos WHERE nombre_producto = N'Casado con pollo a la plancha')
BEGIN
    INSERT INTO dbo.Productos (id_categoria_prod, nombre_producto, descripcion, precio_venta, disponible, estado) VALUES
        (@idCatPlatos, N'Casado con pollo a la plancha', N'Pechuga a la plancha acompañada de arroz, frijoles y ensalada.', 4850.00, 1, 1),
        (@idCatPlatos, N'Hamburguesa clásica El Colibrí', N'Torta de res 200g, queso fundido, vegetales y papas.', 5200.00, 1, 1),
        (@idCatBebidasCalientes, N'Café cappuccino gourmet', N'Café espresso recién preparado con espuma de leche cremosa.', 1950.00, 1, 1),
        (@idCatEntradas, N'Chifrijo especial', N'Frijoles tiernos con arroz, chicharrón, pico de gallo y chips.', 4500.00, 1, 1);
END
GO

-- Recetas y RecetaInsumo (Fundamental para probar Caso C9 / D-08 de descuento de inventario)
DECLARE @prodCasado INT = (SELECT TOP 1 id_producto FROM dbo.Productos WHERE nombre_producto LIKE '%Casado%');
DECLARE @prodBurger INT = (SELECT TOP 1 id_producto FROM dbo.Productos WHERE nombre_producto LIKE '%Hamburguesa%');
DECLARE @prodCafe INT = (SELECT TOP 1 id_producto FROM dbo.Productos WHERE nombre_producto LIKE '%Caf%');

DECLARE @insPollo INT = (SELECT TOP 1 id_insumo FROM dbo.Insumos WHERE nombre_insumo LIKE '%pollo%');
DECLARE @insArroz INT = (SELECT TOP 1 id_insumo FROM dbo.Insumos WHERE nombre_insumo LIKE '%Arroz%');
DECLARE @insFrijol INT = (SELECT TOP 1 id_insumo FROM dbo.Insumos WHERE nombre_insumo LIKE '%Frijol%');
DECLARE @insCarne INT = (SELECT TOP 1 id_insumo FROM dbo.Insumos WHERE nombre_insumo LIKE '%Carne%');
DECLARE @insQueso INT = (SELECT TOP 1 id_insumo FROM dbo.Insumos WHERE nombre_insumo LIKE '%mozzarella%');
DECLARE @insCafe INT = (SELECT TOP 1 id_insumo FROM dbo.Insumos WHERE nombre_insumo LIKE '%Caf%' OR id_insumo = 11);

IF @prodCasado IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Receta WHERE id_producto = @prodCasado)
        INSERT INTO dbo.Receta (id_producto, estado) VALUES (@prodCasado, 1);

    DECLARE @idReceta1 INT = (SELECT TOP 1 id_receta FROM dbo.Receta WHERE id_producto = @prodCasado);
    IF NOT EXISTS (SELECT 1 FROM dbo.RecetaInsumo WHERE id_receta = @idReceta1)
    BEGIN
        IF @insPollo IS NOT NULL INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES (@idReceta1, @insPollo, 0.2000);
        IF @insArroz IS NOT NULL INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES (@idReceta1, @insArroz, 0.1500);
        IF @insFrijol IS NOT NULL INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES (@idReceta1, @insFrijol, 0.1500);
    END
END

IF @prodBurger IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Receta WHERE id_producto = @prodBurger)
        INSERT INTO dbo.Receta (id_producto, estado) VALUES (@prodBurger, 1);

    DECLARE @idReceta2 INT = (SELECT TOP 1 id_receta FROM dbo.Receta WHERE id_producto = @prodBurger);
    IF NOT EXISTS (SELECT 1 FROM dbo.RecetaInsumo WHERE id_receta = @idReceta2)
    BEGIN
        IF @insCarne IS NOT NULL INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES (@idReceta2, @insCarne, 0.2000);
        IF @insQueso IS NOT NULL INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES (@idReceta2, @insQueso, 0.0500);
    END
END

IF @prodCafe IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.Receta WHERE id_producto = @prodCafe)
        INSERT INTO dbo.Receta (id_producto, estado) VALUES (@prodCafe, 1);

    DECLARE @idReceta3 INT = (SELECT TOP 1 id_receta FROM dbo.Receta WHERE id_producto = @prodCafe);
    IF NOT EXISTS (SELECT 1 FROM dbo.RecetaInsumo WHERE id_receta = @idReceta3)
    BEGIN
        IF @insCafe IS NOT NULL INSERT INTO dbo.RecetaInsumo (id_receta, id_insumo, cantidad_usar) VALUES (@idReceta3, @insCafe, 0.0200);
    END
END
GO

PRINT '================================================================';
PRINT ' INSTALACIÓN Y CONFIGURACIÓN COMPLETADA CON ÉXITO ';
PRINT ' Usuarios creados:';
PRINT '   - admin (Rol: Administrador, Clave: Password1234.)';
PRINT '   - cajero_demo (Rol: Cajero, Clave: Password1234.)';
PRINT '   - mesero_demo (Rol: Mesero, Clave: Password1234.)';
PRINT '   - cocinero_demo (Rol: Cocinero, Clave: Password1234.)';
PRINT '   - supervisor_demo (Rol: Supervisor, Clave: Password1234.)';
PRINT '================================================================';
GO
