-- ======================================================================
-- BASE DE DATOS: COLIBRI - RESTAURANTE EL COLIBRI
-- Script actualizado a partir del esquema real de la BD (contraste automatico)
-- Fecha de regeneracion del esquema: 2026-08-04 20:42
-- ======================================================================

IF DB_ID('COLIBRI') IS NULL
BEGIN
    CREATE DATABASE [COLIBRI];
END
GO

USE [COLIBRI];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- ------------------------------------------------------------
-- TABLA: Apertura_Caja
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Apertura_Caja', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Apertura_Caja] (
    [id_apertura] INT IDENTITY(1,1) NOT NULL,
    [id_caja] INT  NOT NULL,
    [id_cajero] INT  NOT NULL,
    [monto_inicial] DECIMAL(12,2)  NOT NULL,
    [fecha_apertura] DATETIME2(7)  NOT NULL,
    [observaciones] NVARCHAR(300)  NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Apertura_Caja] PRIMARY KEY CLUSTERED (id_apertura)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Asistencia
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Asistencia', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Asistencia] (
    [id_asistencia] INT IDENTITY(1,1) NOT NULL,
    [id_empleado] INT  NOT NULL,
    [id_turno] INT  NULL,
    [fecha_hora_entrada] DATETIME2(7)  NOT NULL,
    [fecha_hora_salida] DATETIME2(7)  NULL,
    [observaciones] NVARCHAR(300)  NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Asistencia] PRIMARY KEY CLUSTERED (id_asistencia)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Bitacora_Acceso
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Bitacora_Acceso', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Acceso] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT  NOT NULL,
    [accion] NVARCHAR(50)  NOT NULL,
    [valor_anterior] NVARCHAR(MAX)  NULL,
    [valor_nuevo] NVARCHAR(MAX)  NULL,
    [detalle] NVARCHAR(500)  NULL,
    [ip_origen] NVARCHAR(50)  NULL,
    [dispositivo] NVARCHAR(100)  NULL,
    [fecha_hora] DATETIME2(7)  NOT NULL,
    CONSTRAINT [PK_Bitacora_Acceso] PRIMARY KEY CLUSTERED (id_registro)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Bitacora_Financiera
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Bitacora_Financiera', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Financiera] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT  NOT NULL,
    [tabla_afectada] NVARCHAR(80)  NOT NULL,
    [id_registro_afectado] INT  NOT NULL,
    [accion] NVARCHAR(30)  NOT NULL,
    [valor_anterior] NVARCHAR(MAX)  NULL,
    [valor_nuevo] NVARCHAR(MAX)  NULL,
    [detalle] NVARCHAR(500)  NULL,
    [ip_origen] NVARCHAR(50)  NULL,
    [dispositivo] NVARCHAR(100)  NULL,
    [fecha_hora] DATETIME2(7)  NOT NULL,
    CONSTRAINT [PK_Bitacora_Financiera] PRIMARY KEY CLUSTERED (id_registro)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Bitacora_Inventario
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Bitacora_Inventario', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Inventario] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT  NOT NULL,
    [accion] NVARCHAR(30)  NOT NULL,
    [valor_anterior] NVARCHAR(MAX)  NULL,
    [valor_nuevo] NVARCHAR(MAX)  NULL,
    [detalle] NVARCHAR(500)  NULL,
    [ip_origen] NVARCHAR(50)  NULL,
    [dispositivo] NVARCHAR(100)  NULL,
    [fecha_hora] DATETIME2(7)  NOT NULL,
    CONSTRAINT [PK_Bitacora_Inventario] PRIMARY KEY CLUSTERED (id_registro)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Bitacora_PDV
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Bitacora_PDV', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_PDV] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_caja] INT  NOT NULL,
    [id_usuario_cajero] INT  NOT NULL,
    [accion_operativa] NVARCHAR(50)  NOT NULL,
    [id_venta] INT  NULL,
    [detalle] NVARCHAR(MAX)  NULL,
    [fecha_hora] DATETIME2(7)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Bitacora_PDV] PRIMARY KEY CLUSTERED (id_registro)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Bitacora_Pedidos
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Bitacora_Pedidos', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Pedidos] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT  NOT NULL,
    [accion] NVARCHAR(30)  NOT NULL,
    [estado_anterior] NVARCHAR(MAX)  NULL,
    [estado_nuevo] NVARCHAR(MAX)  NULL,
    [detalle] NVARCHAR(500)  NULL,
    [ip_origen] NVARCHAR(50)  NULL,
    [dispositivo] NVARCHAR(100)  NULL,
    [fecha_hora] DATETIME2(7)  NOT NULL,
    [id_pedido] INT  NOT NULL,
    CONSTRAINT [PK_Bitacora_Pedidos] PRIMARY KEY CLUSTERED (id_registro)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Bitacora_Reportes
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Bitacora_Reportes', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Reportes] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT  NOT NULL,
    [accion] NVARCHAR(30)  NOT NULL,
    [valor_anterior] NVARCHAR(MAX)  NULL,
    [valor_nuevo] NVARCHAR(MAX)  NULL,
    [detalle] NVARCHAR(500)  NULL,
    [ip_origen] NVARCHAR(50)  NULL,
    [dispositivo] NVARCHAR(100)  NULL,
    [fecha_hora] DATETIME2(7)  NOT NULL,
    CONSTRAINT [PK_Bitacora_Reportes] PRIMARY KEY CLUSTERED (id_registro)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Bitacora_Usuarios_RRHH
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Bitacora_Usuarios_RRHH', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Bitacora_Usuarios_RRHH] (
    [id_registro] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT  NOT NULL,
    [tabla_afectada] NVARCHAR(80)  NOT NULL,
    [id_registro_afectado] INT  NOT NULL,
    [accion] NVARCHAR(30)  NOT NULL,
    [valor_anterior] NVARCHAR(MAX)  NULL,
    [valor_nuevo] NVARCHAR(MAX)  NULL,
    [detalle] NVARCHAR(500)  NULL,
    [ip_origen] NVARCHAR(50)  NULL,
    [dispositivo] NVARCHAR(100)  NULL,
    [fecha_hora] DATETIME2(7)  NOT NULL,
    CONSTRAINT [PK_Bitacora_RRHH] PRIMARY KEY CLUSTERED (id_registro)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Cajas
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Cajas', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Cajas] (
    [id_caja] INT IDENTITY(1,1) NOT NULL,
    [nombre_caja] NVARCHAR(50)  NOT NULL,
    [estado_caja] NVARCHAR(20)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Cajas] PRIMARY KEY CLUSTERED (id_caja)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Categorias_Insumo
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Categorias_Insumo', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Categorias_Insumo] (
    [id_categoria] INT IDENTITY(1,1) NOT NULL,
    [nombre_categoria] NVARCHAR(100)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Categorias_Insumo] PRIMARY KEY CLUSTERED (id_categoria)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Categorias_Producto
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Categorias_Producto', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Categorias_Producto] (
    [id_categoria_prod] INT IDENTITY(1,1) NOT NULL,
    [nombre_categoria] NVARCHAR(100)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Categorias_Producto] PRIMARY KEY CLUSTERED (id_categoria_prod)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Cierres_Caja
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Cierres_Caja', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Cierres_Caja] (
    [id_cierre] INT IDENTITY(1,1) NOT NULL,
    [id_cajero] INT  NOT NULL,
    [id_apertura] INT  NOT NULL,
    [fecha_cierre] DATETIME2(7)  NOT NULL,
    [monto_apertura] DECIMAL(12,2)  NOT NULL,
    [total_efectivo] DECIMAL(12,2)  NOT NULL,
    [total_sinpe] DECIMAL(12,2)  NOT NULL,
    [total_tarjeta] DECIMAL(12,2)  NOT NULL,
    [total_egresos] DECIMAL(12,2)  NOT NULL,
    [saldo_esperado] DECIMAL(12,2)  NOT NULL,
    [saldo_real] DECIMAL(12,2)  NOT NULL,
    [descuadre] BIT  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Cierres_Caja] PRIMARY KEY CLUSTERED (id_cierre)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Cierres_Periodo
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Cierres_Periodo', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Cierres_Periodo] (
    [id_cierre_periodo] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT  NOT NULL,
    [tipo_periodo] NVARCHAR(10)  NOT NULL,
    [mes] INT  NULL,
    [anio] INT  NOT NULL,
    [fecha_cierre] DATETIME2(7)  NOT NULL,
    [total_ingresos] DECIMAL(12,2)  NOT NULL,
    [total_egresos] DECIMAL(12,2)  NOT NULL,
    [total_notas_credito] DECIMAL(12,2)  NOT NULL,
    [saldo_final] DECIMAL(12,2)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Cierres_Periodo] PRIMARY KEY CLUSTERED (id_cierre_periodo)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Detalle_Pedido
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Detalle_Pedido', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Detalle_Pedido] (
    [id_detalle] INT IDENTITY(1,1) NOT NULL,
    [id_pedido] INT  NOT NULL,
    [id_producto] INT  NOT NULL,
    [cantidad] DECIMAL(10,2)  NOT NULL,
    [precio_unitario] DECIMAL(10,2)  NOT NULL,
    [observaciones_item] NVARCHAR(300)  NULL,
    [estado_item] NVARCHAR(20)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Detalle_Pedido] PRIMARY KEY CLUSTERED (id_detalle)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Detalle_Venta
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Detalle_Venta', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Detalle_Venta] (
    [id_detalle_venta] INT IDENTITY(1,1) NOT NULL,
    [id_venta] INT  NOT NULL,
    [id_producto] INT  NOT NULL,
    [cantidad] INT  NOT NULL,
    [precio_unitario] DECIMAL(10,2)  NOT NULL,
    [subtotal_item] DECIMAL(10,2)  NOT NULL,
    [observaciones_item] NVARCHAR(MAX)  NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Detalle_Venta] PRIMARY KEY CLUSTERED (id_detalle_venta)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Egresos_Caja
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Egresos_Caja', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Egresos_Caja] (
    [id_egreso] INT IDENTITY(1,1) NOT NULL,
    [id_apertura] INT  NOT NULL,
    [id_usuario] INT  NOT NULL,
    [categoria_gasto] NVARCHAR(100)  NOT NULL,
    [descripcion] NVARCHAR(300)  NULL,
    [monto] DECIMAL(12,2)  NOT NULL,
    [fecha_hora] DATETIME2(7)  NOT NULL,
    [estado] BIT  NOT NULL,
    [metodo_pago] NVARCHAR(20)  NOT NULL,
    CONSTRAINT [PK_Egresos_Caja] PRIMARY KEY CLUSTERED (id_egreso)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Empleados
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Empleados', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Empleados] (
    [id_empleado] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT  NOT NULL,
    [cedula] NVARCHAR(20)  NOT NULL,
    [nombre] NVARCHAR(80)  NOT NULL,
    [apellidos] NVARCHAR(120)  NOT NULL,
    [telefono] NVARCHAR(20)  NULL,
    [correo_personal] NVARCHAR(150)  NULL,
    [salario_hora] DECIMAL(10,2)  NOT NULL,
    [dias_vacaciones_disponibles] DECIMAL(6,2)  NOT NULL,
    [fecha_ingreso] DATE  NOT NULL,
    [fecha_modificacion] DATETIME2(7)  NULL,
    [fecha_reactivacion] DATETIME2(7)  NULL,
    [motivo_inactivacion] NVARCHAR(300)  NULL,
    [estado] BIT  NOT NULL,
    [direccion] NVARCHAR(300)  NULL,
    CONSTRAINT [PK_Empleados] PRIMARY KEY CLUSTERED (id_empleado)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Envios_PDV
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Envios_PDV', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Envios_PDV] (
    [id_envio] INT IDENTITY(1,1) NOT NULL,
    [id_venta] INT  NOT NULL,
    [id_usuario] INT  NOT NULL,
    [destino] NVARCHAR(20)  NOT NULL,
    [prioridad] NVARCHAR(10)  NOT NULL,
    [estado_envio] NVARCHAR(12)  NOT NULL,
    [codigo_confirmacion] NVARCHAR(50)  NULL,
    [observaciones] NVARCHAR(500)  NULL,
    [motivo_reenvio] NVARCHAR(300)  NULL,
    [fecha_hora_envio] DATETIME2(7)  NOT NULL,
    CONSTRAINT [PK_Envios_PDV] PRIMARY KEY CLUSTERED (id_envio)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Historial_Estados_Pedido
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Historial_Estados_Pedido', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Historial_Estados_Pedido] (
    [id_historial] INT IDENTITY(1,1) NOT NULL,
    [id_pedido] INT  NOT NULL,
    [estado_anterior] NVARCHAR(30)  NULL,
    [estado_nuevo] NVARCHAR(30)  NOT NULL,
    [usuario_responsable] NVARCHAR(100)  NULL,
    [fecha_hora_cambio] DATETIME2(7)  NOT NULL,
    [detalle] NVARCHAR(500)  NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK__Historia__76E6C502CD66F0FD] PRIMARY KEY CLUSTERED (id_historial)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Horas_Extra
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Horas_Extra', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Horas_Extra] (
    [id_hora_extra] INT IDENTITY(1,1) NOT NULL,
    [id_asistencia] INT  NOT NULL,
    [cantidad_horas] DECIMAL(5,2)  NOT NULL,
    [factor_pago] DECIMAL(4,2)  NOT NULL,
    [motivo_ajuste] NVARCHAR(300)  NULL,
    [fecha_registro] DATETIME2(7)  NOT NULL,
    [estado] BIT  NOT NULL,
    [monto_calculado] DECIMAL(12,2)  NULL,
    CONSTRAINT [PK_Horas_Extra] PRIMARY KEY CLUSTERED (id_hora_extra)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Insumos
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Insumos', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Insumos] (
    [id_insumo] INT IDENTITY(1,1) NOT NULL,
    [id_categoria] INT  NOT NULL,
    [nombre_insumo] NVARCHAR(120)  NOT NULL,
    [unidad_medida] NVARCHAR(30)  NOT NULL,
    [stock_minimo] DECIMAL(10,2)  NOT NULL,
    [stock_actual] DECIMAL(10,2)  NOT NULL,
    [costo_unitario] DECIMAL(10,2)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Insumos] PRIMARY KEY CLUSTERED (id_insumo)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Insumos_Proveedores
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Insumos_Proveedores', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Insumos_Proveedores] (
    [id_insumo] INT  NOT NULL,
    [id_proveedor] INT  NOT NULL,
    [fecha_asoc] DATETIME2(7)  NOT NULL,
    CONSTRAINT [PK_Insumos_Proveedores] PRIMARY KEY CLUSTERED (id_insumo, id_proveedor)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Mesas
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Mesas', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Mesas] (
    [id_mesa] INT IDENTITY(1,1) NOT NULL,
    [numero_mesa] NVARCHAR(10)  NOT NULL,
    [capacidad] TINYINT  NOT NULL,
    [estado_mesa] NVARCHAR(20)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Mesas] PRIMARY KEY CLUSTERED (id_mesa)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Nomina_Mensual
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Nomina_Mensual', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Nomina_Mensual] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [empleado_id] INT  NOT NULL,
    [mes] INT  NOT NULL,
    [anio] INT  NOT NULL,
    [horas_trabajadas] DECIMAL(10,2)  NOT NULL,
    [horas_extra] DECIMAL(10,2)  NOT NULL,
    [vacaciones_pagadas] DECIMAL(5,2)  NOT NULL,
    [horas_por_dia_vacacion] DECIMAL(5,2)  NOT NULL,
    [monto_vacaciones] DECIMAL(18,2)  NOT NULL,
    [dias_trabajados] INT  NOT NULL,
    [dias_ausentes] INT  NOT NULL,
    [valor_hora] DECIMAL(18,2)  NOT NULL,
    [salario_base] DECIMAL(18,2)  NOT NULL,
    [monto_horas_extra] DECIMAL(18,2)  NOT NULL,
    [bonificaciones] DECIMAL(18,2)  NOT NULL,
    [deducciones] DECIMAL(18,2)  NOT NULL,
    [salario_bruto] DECIMAL(18,2)  NOT NULL,
    [salario_neto] DECIMAL(18,2)  NOT NULL,
    [estado] NVARCHAR(20)  NOT NULL,
    [fecha_cierre] DATETIME  NULL,
    [observaciones] NVARCHAR(500)  NULL,
    [creado_por] INT  NOT NULL,
    [created_at] DATETIME  NOT NULL,
    [updated_at] DATETIME  NOT NULL,
    CONSTRAINT [PK_nomina_mensual] PRIMARY KEY CLUSTERED (id)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Notas_Credito
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Notas_Credito', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Notas_Credito] (
    [id_nota_credito] INT IDENTITY(1,1) NOT NULL,
    [id_venta] INT  NOT NULL,
    [id_usuario] INT  NOT NULL,
    [motivo] NVARCHAR(300)  NOT NULL,
    [monto] DECIMAL(12,2)  NOT NULL,
    [fecha_hora] DATETIME2(7)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Notas_Credito] PRIMARY KEY CLUSTERED (id_nota_credito)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: PasswordHistorial
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.PasswordHistorial', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[PasswordHistorial] (
    [id_historial] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT  NOT NULL,
    [password_hash] NVARCHAR(255)  NOT NULL,
    [fecha_cambio] DATETIME2(7)  NOT NULL,
    [metodo_cambio] NVARCHAR(20)  NOT NULL,
    [dispositivo] NVARCHAR(100)  NULL,
    [direccion_ip] NVARCHAR(50)  NULL,
    CONSTRAINT [PK_PasswordHistorial] PRIMARY KEY CLUSTERED (id_historial)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Pedidos
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Pedidos', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Pedidos] (
    [id_pedido] INT IDENTITY(1,1) NOT NULL,
    [id_mesa] INT  NULL,
    [id_empleado] INT  NOT NULL,
    [tipo_servicio] NVARCHAR(30)  NOT NULL,
    [cantidad_comensales] TINYINT  NULL,
    [estado_pedido] NVARCHAR(30)  NOT NULL,
    [fecha_hora] DATETIME2(7)  NOT NULL,
    [observaciones] NVARCHAR(500)  NULL,
    [estado] BIT  NOT NULL,
    [fecha_hora_entrega] DATETIME2(7)  NULL,
    [fecha_hora_finalizacion] DATETIME2(7)  NULL,
    CONSTRAINT [PK_Pedidos] PRIMARY KEY CLUSTERED (id_pedido)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Productos
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Productos', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Productos] (
    [id_producto] INT IDENTITY(1,1) NOT NULL,
    [id_categoria_prod] INT  NOT NULL,
    [nombre_producto] NVARCHAR(120)  NOT NULL,
    [descripcion] NVARCHAR(500)  NULL,
    [precio_venta] DECIMAL(10,2)  NOT NULL,
    [disponible] BIT  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Productos] PRIMARY KEY CLUSTERED (id_producto)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Proveedores
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Proveedores', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Proveedores] (
    [id_proveedor] INT IDENTITY(1,1) NOT NULL,
    [cedula_juridica] NVARCHAR(50)  NOT NULL,
    [nombre_empresa] NVARCHAR(150)  NOT NULL,
    [contacto_nombre] NVARCHAR(100)  NULL,
    [telefono] NVARCHAR(20)  NULL,
    [correo] NVARCHAR(150)  NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Proveedores] PRIMARY KEY CLUSTERED (id_proveedor)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Receta
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Receta', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Receta] (
    [id_receta] INT IDENTITY(1,1) NOT NULL,
    [id_producto] INT  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Receta] PRIMARY KEY CLUSTERED (id_receta)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: RecetaInsumo
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.RecetaInsumo', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[RecetaInsumo] (
    [id_receta] INT  NOT NULL,
    [id_insumo] INT  NOT NULL,
    [cantidad_usar] DECIMAL(10,4)  NOT NULL,
    CONSTRAINT [PK_RecetaInsumo] PRIMARY KEY CLUSTERED (id_receta, id_insumo)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Reportes_Generados
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Reportes_Generados', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Reportes_Generados] (
    [id_reporte] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT  NOT NULL,
    [tipo_reporte] NVARCHAR(50)  NOT NULL,
    [parametros] NVARCHAR(500)  NULL,
    [formato_salida] NVARCHAR(10)  NOT NULL,
    [fecha_generacion] DATETIME2(7)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Reportes] PRIMARY KEY CLUSTERED (id_reporte)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Roles
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Roles', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Roles] (
    [id_rol] INT IDENTITY(1,1) NOT NULL,
    [nombre_rol] NVARCHAR(60)  NOT NULL,
    [descripcion] NVARCHAR(255)  NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED (id_rol)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Sesiones
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Sesiones', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Sesiones] (
    [id_sesion] INT IDENTITY(1,1) NOT NULL,
    [id_usuario] INT  NOT NULL,
    [estado_sesion] NVARCHAR(20)  NOT NULL,
    [fecha_hora_inicio] DATETIME2(7)  NOT NULL,
    [fecha_hora_ultima_act] DATETIME2(7)  NOT NULL,
    [fecha_hora_cierre] DATETIME2(7)  NULL,
    [dispositivo_acceso] NVARCHAR(100)  NULL,
    [direccion_ip] NVARCHAR(50)  NULL,
    [motivo_cierre] NVARCHAR(50)  NULL,
    CONSTRAINT [PK_Sesiones] PRIMARY KEY CLUSTERED (id_sesion)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Subcuenta_Detalle_Pedido
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Subcuenta_Detalle_Pedido', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Subcuenta_Detalle_Pedido] (
    [id_subcuenta_detalle] INT IDENTITY(1,1) NOT NULL,
    [id_subcuenta] INT  NOT NULL,
    [id_detalle_pedido] INT  NOT NULL,
    [cantidad] DECIMAL(10,2)  NOT NULL,
    [subtotal] DECIMAL(12,2)  NOT NULL,
    [fecha_operacion] DATETIME2(7)  NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Subcuenta_Detalle] PRIMARY KEY CLUSTERED (id_subcuenta_detalle)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Subcuenta_Detalle_Venta
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Subcuenta_Detalle_Venta', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Subcuenta_Detalle_Venta] (
    [id_subcuenta_detalle] INT IDENTITY(1,1) NOT NULL,
    [id_subcuenta] INT  NOT NULL,
    [id_detalle_venta] INT  NOT NULL,
    [cantidad] INT  NOT NULL,
    [subtotal] DECIMAL(12,2)  NOT NULL,
    [fecha_operacion] DATETIME2(7)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Subcuenta_Detalle_Venta] PRIMARY KEY CLUSTERED (id_subcuenta_detalle)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Subcuentas_Pedido
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Subcuentas_Pedido', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Subcuentas_Pedido] (
    [id_subcuenta] INT IDENTITY(1,1) NOT NULL,
    [id_pedido] INT  NOT NULL,
    [nombre_subcuenta] NVARCHAR(50)  NOT NULL,
    [estado] BIT  NOT NULL,
    [fecha_operacion] DATETIME2(7)  NULL,
    CONSTRAINT [PK_Subcuentas_Pedido] PRIMARY KEY CLUSTERED (id_subcuenta)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Subcuentas_Venta
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Subcuentas_Venta', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Subcuentas_Venta] (
    [id_subcuenta] INT IDENTITY(1,1) NOT NULL,
    [id_venta] INT  NOT NULL,
    [nombre_subcuenta] NVARCHAR(100)  NOT NULL,
    [subtotal] DECIMAL(12,2)  NOT NULL,
    [pagada] BIT  NOT NULL,
    [fecha_operacion] DATETIME2(7)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Subcuentas_Venta] PRIMARY KEY CLUSTERED (id_subcuenta)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Turnos_Trabajo
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Turnos_Trabajo', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Turnos_Trabajo] (
    [id_turno] INT IDENTITY(1,1) NOT NULL,
    [id_empleado] INT  NOT NULL,
    [fecha_turno] DATE  NOT NULL,
    [hora_inicio] TIME(7)  NOT NULL,
    [hora_fin] TIME(7)  NOT NULL,
    [descripcion] NVARCHAR(200)  NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Turnos_Trabajo] PRIMARY KEY CLUSTERED (id_turno)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Usuarios
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Usuarios] (
    [id_usuario] INT IDENTITY(1,1) NOT NULL,
    [id_rol] INT  NOT NULL,
    [nombre_usuario] NVARCHAR(80)  NOT NULL,
    [correo] NVARCHAR(150)  NOT NULL,
    [password_hash] NVARCHAR(255)  NOT NULL,
    [cambio_password_requerido] BIT  NOT NULL,
    [intentos_fallidos] TINYINT  NOT NULL,
    [bloqueado] BIT  NOT NULL,
    [fecha_ultimo_acceso] DATETIME2(7)  NULL,
    [fecha_password] DATETIME2(7)  NULL,
    [estado] BIT  NOT NULL,
    [fecha_creacion] DATETIME2(7)  NOT NULL,
    [fecha_aviso_password] DATETIME2(7)  NULL,
    [ultimo_cambio_password_ip] NVARCHAR(50)  NULL,
    [ultimo_cambio_password_dispositivo] NVARCHAR(100)  NULL,
    [direccion] NVARCHAR(300)  NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED (id_usuario)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Vacaciones
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Vacaciones', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Vacaciones] (
    [id_vacacion] INT IDENTITY(1,1) NOT NULL,
    [id_empleado] INT  NOT NULL,
    [id_aprobador] INT  NULL,
    [fecha_inicio] DATE  NOT NULL,
    [fecha_fin] DATE  NOT NULL,
    [dias_solicitados] DECIMAL(5,1)  NOT NULL,
    [estado_solicitud] NVARCHAR(20)  NOT NULL,
    [motivo_rechazo] NVARCHAR(300)  NULL,
    [fecha_solicitud] DATETIME2(7)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Vacaciones] PRIMARY KEY CLUSTERED (id_vacacion)
)
END
GO

-- ------------------------------------------------------------
-- TABLA: Ventas
-- ------------------------------------------------------------
IF OBJECT_ID(N'dbo.Ventas', N'U') IS NULL
BEGIN
CREATE TABLE [dbo].[Ventas] (
    [id_venta] INT IDENTITY(1,1) NOT NULL,
    [id_pedido] INT  NULL,
    [id_subcuenta] INT  NULL,
    [id_empleado] INT  NOT NULL,
    [id_apertura] INT  NULL,
    [tipo_venta] NVARCHAR(30)  NOT NULL,
    [total_cobrado] DECIMAL(12,2)  NOT NULL,
    [monto_recibido] DECIMAL(12,2)  NOT NULL,
    [vuelto] DECIMAL(12,2)  NOT NULL,
    [metodo_pago] NVARCHAR(30)  NOT NULL,
    [estado_venta] NVARCHAR(20)  NOT NULL,
    [fecha_hora] DATETIME2(7)  NOT NULL,
    [estado] BIT  NOT NULL,
    CONSTRAINT [PK_Ventas] PRIMARY KEY CLUSTERED (id_venta)
)
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Aper_fecha')
    ALTER TABLE [dbo].[Apertura_Caja] ADD CONSTRAINT [DF_Aper_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_apertura];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Aper_estado')
    ALTER TABLE [dbo].[Apertura_Caja] ADD CONSTRAINT [DF_Aper_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Aper_monto')
    ALTER TABLE [dbo].[Apertura_Caja] ADD CONSTRAINT [CK_Aper_monto] CHECK ([monto_inicial]>=(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Aper_Cajero')
    ALTER TABLE [dbo].[Apertura_Caja] WITH CHECK ADD CONSTRAINT [FK_Aper_Cajero] FOREIGN KEY([id_cajero])
    REFERENCES [dbo].[Empleados] ([id_empleado]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Aper_Caja')
    ALTER TABLE [dbo].[Apertura_Caja] WITH CHECK ADD CONSTRAINT [FK_Aper_Caja] FOREIGN KEY([id_caja])
    REFERENCES [dbo].[Cajas] ([id_caja]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Asist_entrada')
    ALTER TABLE [dbo].[Asistencia] ADD CONSTRAINT [DF_Asist_entrada] DEFAULT (sysutcdatetime()) FOR [fecha_hora_entrada];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Asist_estado')
    ALTER TABLE [dbo].[Asistencia] ADD CONSTRAINT [DF_Asist_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Asist_Empleado')
    ALTER TABLE [dbo].[Asistencia] WITH CHECK ADD CONSTRAINT [FK_Asist_Empleado] FOREIGN KEY([id_empleado])
    REFERENCES [dbo].[Empleados] ([id_empleado]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Asist_Turno')
    ALTER TABLE [dbo].[Asistencia] WITH CHECK ADD CONSTRAINT [FK_Asist_Turno] FOREIGN KEY([id_turno])
    REFERENCES [dbo].[Turnos_Trabajo] ([id_turno]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Asistencia_empleado' AND object_id = OBJECT_ID('Asistencia'))
    CREATE  NONCLUSTERED INDEX [IX_Asistencia_empleado] ON [dbo].[Asistencia] (id_empleado, fecha_hora_entrada);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UQ_Asistencia_Registro' AND object_id = OBJECT_ID('Asistencia'))
    CREATE UNIQUE  NONCLUSTERED INDEX [UQ_Asistencia_Registro] ON [dbo].[Asistencia] (id_empleado, id_turno) WHERE ([id_turno] IS NOT NULL AND [estado]=(1));
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_BitAcc_fecha')
    ALTER TABLE [dbo].[Bitacora_Acceso] ADD CONSTRAINT [DF_BitAcc_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_hora];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_BitAcc_evento')
    ALTER TABLE [dbo].[Bitacora_Acceso] ADD CONSTRAINT [CK_BitAcc_evento] CHECK ([accion]='LOGIN_SUCCESS' OR [accion]='PASSWORD_RESET' OR [accion]='FORGOT_PASSWORD' OR [accion]='INTENTO_FALLIDO' OR [accion]='SESION_EXPIRADA' OR [accion]='BLOQUEO' OR [accion]='LOGOUT' OR [accion]='LOGIN');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BitAcc_Usuario')
    ALTER TABLE [dbo].[Bitacora_Acceso] WITH CHECK ADD CONSTRAINT [FK_BitAcc_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BitAcc_usuario' AND object_id = OBJECT_ID('Bitacora_Acceso'))
    CREATE  NONCLUSTERED INDEX [IX_BitAcc_usuario] ON [dbo].[Bitacora_Acceso] (id_usuario, fecha_hora);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_BitFin_fecha')
    ALTER TABLE [dbo].[Bitacora_Financiera] ADD CONSTRAINT [DF_BitFin_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_hora];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_BitFin_accion')
    ALTER TABLE [dbo].[Bitacora_Financiera] ADD CONSTRAINT [CK_BitFin_accion] CHECK ([accion]='DELETE' OR [accion]='UPDATE' OR [accion]='INSERT' OR [accion]='ANULACION' OR [accion]='REENVIO' OR [accion]='ENVIO' OR [accion]='CANCELACION' OR [accion]='COBRO' OR [accion]='VALIDACION' OR [accion]='EDICION' OR [accion]='APERTURA_VENTA');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BitFin_Usuario')
    ALTER TABLE [dbo].[Bitacora_Financiera] WITH CHECK ADD CONSTRAINT [FK_BitFin_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BitFin_fecha' AND object_id = OBJECT_ID('Bitacora_Financiera'))
    CREATE  NONCLUSTERED INDEX [IX_BitFin_fecha] ON [dbo].[Bitacora_Financiera] (fecha_hora);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_BitInv_fecha')
    ALTER TABLE [dbo].[Bitacora_Inventario] ADD CONSTRAINT [DF_BitInv_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_hora];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_BitInv_accion')
    ALTER TABLE [dbo].[Bitacora_Inventario] ADD CONSTRAINT [CK_BitInv_accion] CHECK ([accion]='activacion' OR [accion]='baja' OR [accion]='ajuste' OR [accion]='salida' OR [accion]='entrada');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BitInv_Usuario')
    ALTER TABLE [dbo].[Bitacora_Inventario] WITH CHECK ADD CONSTRAINT [FK_BitInv_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BitInv_fecha' AND object_id = OBJECT_ID('Bitacora_Inventario'))
    CREATE  NONCLUSTERED INDEX [IX_BitInv_fecha] ON [dbo].[Bitacora_Inventario] (fecha_hora);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_BitPed_fecha')
    ALTER TABLE [dbo].[Bitacora_Pedidos] ADD CONSTRAINT [DF_BitPed_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_hora];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_BitPed_accion')
    ALTER TABLE [dbo].[Bitacora_Pedidos] ADD CONSTRAINT [CK_BitPed_accion] CHECK ([accion]='PAGO' OR [accion]='ENTREGA' OR [accion]='CANCELACION' OR [accion]='MODIFICACION' OR [accion]='CREACION');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BitPed_Usuario')
    ALTER TABLE [dbo].[Bitacora_Pedidos] WITH CHECK ADD CONSTRAINT [FK_BitPed_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BitPed_pedido' AND object_id = OBJECT_ID('Bitacora_Pedidos'))
    CREATE  NONCLUSTERED INDEX [IX_BitPed_pedido] ON [dbo].[Bitacora_Pedidos] (fecha_hora);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_BitRep_fecha')
    ALTER TABLE [dbo].[Bitacora_Reportes] ADD CONSTRAINT [DF_BitRep_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_hora];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_BitRep_accion')
    ALTER TABLE [dbo].[Bitacora_Reportes] ADD CONSTRAINT [CK_BitRep_accion] CHECK ([accion]='VISUALIZACION' OR [accion]='DESCARGA' OR [accion]='GENERACION');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BitRep_Usuario')
    ALTER TABLE [dbo].[Bitacora_Reportes] WITH CHECK ADD CONSTRAINT [FK_BitRep_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_BitRR_fecha')
    ALTER TABLE [dbo].[Bitacora_Usuarios_RRHH] ADD CONSTRAINT [DF_BitRR_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_hora];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_BitRR_accion')
    ALTER TABLE [dbo].[Bitacora_Usuarios_RRHH] ADD CONSTRAINT [CK_BitRR_accion] CHECK ([accion]='ASIGNACION_MESA' OR [accion]='ROLE_ASSIGNMENT' OR [accion]='PASSWORD_CHANGE' OR [accion]='DESACTIVACION' OR [accion]='ACTIVACION' OR [accion]='DELETE' OR [accion]='UPDATE' OR [accion]='INSERT');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_BitRR_Usuario')
    ALTER TABLE [dbo].[Bitacora_Usuarios_RRHH] WITH CHECK ADD CONSTRAINT [FK_BitRR_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_BitRR_fecha' AND object_id = OBJECT_ID('Bitacora_Usuarios_RRHH'))
    CREATE  NONCLUSTERED INDEX [IX_BitRR_fecha] ON [dbo].[Bitacora_Usuarios_RRHH] (fecha_hora);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Cajas_estado_caja')
    ALTER TABLE [dbo].[Cajas] ADD CONSTRAINT [DF_Cajas_estado_caja] DEFAULT ('cerrada') FOR [estado_caja];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Cajas_estado')
    ALTER TABLE [dbo].[Cajas] ADD CONSTRAINT [DF_Cajas_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Cajas_estado')
    ALTER TABLE [dbo].[Cajas] ADD CONSTRAINT [CK_Cajas_estado] CHECK ([estado_caja]='mantenimiento' OR [estado_caja]='cerrada' OR [estado_caja]='abierta');
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Cajas_nombre')
    ALTER TABLE [dbo].[Cajas] ADD CONSTRAINT [UQ_Cajas_nombre] UNIQUE (nombre_caja);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_CatIns_estado')
    ALTER TABLE [dbo].[Categorias_Insumo] ADD CONSTRAINT [DF_CatIns_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_CatIns_nombre')
    ALTER TABLE [dbo].[Categorias_Insumo] ADD CONSTRAINT [UQ_CatIns_nombre] UNIQUE (nombre_categoria);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_CatProd_estado')
    ALTER TABLE [dbo].[Categorias_Producto] ADD CONSTRAINT [DF_CatProd_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_CatProd_nombre')
    ALTER TABLE [dbo].[Categorias_Producto] ADD CONSTRAINT [UQ_CatProd_nombre] UNIQUE (nombre_categoria);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Cier_egresos')
    ALTER TABLE [dbo].[Cierres_Caja] ADD CONSTRAINT [DF_Cier_egresos] DEFAULT ((0)) FOR [total_egresos];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Cier_descuadre')
    ALTER TABLE [dbo].[Cierres_Caja] ADD CONSTRAINT [DF_Cier_descuadre] DEFAULT ((0)) FOR [descuadre];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Cier_estado')
    ALTER TABLE [dbo].[Cierres_Caja] ADD CONSTRAINT [DF_Cier_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Cier_fecha')
    ALTER TABLE [dbo].[Cierres_Caja] ADD CONSTRAINT [DF_Cier_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_cierre];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Cier_apertura')
    ALTER TABLE [dbo].[Cierres_Caja] ADD CONSTRAINT [DF_Cier_apertura] DEFAULT ((0)) FOR [monto_apertura];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Cier_efectivo')
    ALTER TABLE [dbo].[Cierres_Caja] ADD CONSTRAINT [DF_Cier_efectivo] DEFAULT ((0)) FOR [total_efectivo];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Cier_sinpe')
    ALTER TABLE [dbo].[Cierres_Caja] ADD CONSTRAINT [DF_Cier_sinpe] DEFAULT ((0)) FOR [total_sinpe];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Cier_tarjeta')
    ALTER TABLE [dbo].[Cierres_Caja] ADD CONSTRAINT [DF_Cier_tarjeta] DEFAULT ((0)) FOR [total_tarjeta];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Cier_Cajero')
    ALTER TABLE [dbo].[Cierres_Caja] WITH CHECK ADD CONSTRAINT [FK_Cier_Cajero] FOREIGN KEY([id_cajero])
    REFERENCES [dbo].[Empleados] ([id_empleado]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Cier_Apertura')
    ALTER TABLE [dbo].[Cierres_Caja] WITH CHECK ADD CONSTRAINT [FK_Cier_Apertura] FOREIGN KEY([id_apertura])
    REFERENCES [dbo].[Apertura_Caja] ([id_apertura]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cierres_fecha' AND object_id = OBJECT_ID('Cierres_Caja'))
    CREATE  NONCLUSTERED INDEX [IX_Cierres_fecha] ON [dbo].[Cierres_Caja] (fecha_cierre);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Cier_Apertura')
    ALTER TABLE [dbo].[Cierres_Caja] ADD CONSTRAINT [UQ_Cier_Apertura] UNIQUE (id_apertura);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_CPer_fecha')
    ALTER TABLE [dbo].[Cierres_Periodo] ADD CONSTRAINT [DF_CPer_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_cierre];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_CPer_ingresos')
    ALTER TABLE [dbo].[Cierres_Periodo] ADD CONSTRAINT [DF_CPer_ingresos] DEFAULT ((0)) FOR [total_ingresos];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_CPer_egresos')
    ALTER TABLE [dbo].[Cierres_Periodo] ADD CONSTRAINT [DF_CPer_egresos] DEFAULT ((0)) FOR [total_egresos];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_CPer_nc')
    ALTER TABLE [dbo].[Cierres_Periodo] ADD CONSTRAINT [DF_CPer_nc] DEFAULT ((0)) FOR [total_notas_credito];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_CPer_saldo')
    ALTER TABLE [dbo].[Cierres_Periodo] ADD CONSTRAINT [DF_CPer_saldo] DEFAULT ((0)) FOR [saldo_final];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_CPer_estado')
    ALTER TABLE [dbo].[Cierres_Periodo] ADD CONSTRAINT [DF_CPer_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_CPer_tipo')
    ALTER TABLE [dbo].[Cierres_Periodo] ADD CONSTRAINT [CK_CPer_tipo] CHECK ([tipo_periodo]='anual' OR [tipo_periodo]='mensual');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_CPer_mes')
    ALTER TABLE [dbo].[Cierres_Periodo] ADD CONSTRAINT [CK_CPer_mes] CHECK ([mes] IS NULL OR [mes]>=(1) AND [mes]<=(12));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CPer_Usuario')
    ALTER TABLE [dbo].[Cierres_Periodo] WITH CHECK ADD CONSTRAINT [FK_CPer_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_CPer_periodo')
    ALTER TABLE [dbo].[Cierres_Periodo] ADD CONSTRAINT [UQ_CPer_periodo] UNIQUE (tipo_periodo, mes, anio);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Det_estado_i')
    ALTER TABLE [dbo].[Detalle_Pedido] ADD CONSTRAINT [DF_Det_estado_i] DEFAULT ('pendiente') FOR [estado_item];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Det_estado')
    ALTER TABLE [dbo].[Detalle_Pedido] ADD CONSTRAINT [DF_Det_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Det_cantidad')
    ALTER TABLE [dbo].[Detalle_Pedido] ADD CONSTRAINT [CK_Det_cantidad] CHECK ([cantidad]>(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Det_precio')
    ALTER TABLE [dbo].[Detalle_Pedido] ADD CONSTRAINT [CK_Det_precio] CHECK ([precio_unitario]>=(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Det_estado_item')
    ALTER TABLE [dbo].[Detalle_Pedido] ADD CONSTRAINT [CK_Det_estado_item] CHECK ([estado_item]='cancelado' OR [estado_item]='entregado' OR [estado_item]='listo' OR [estado_item]='preparando' OR [estado_item]='pendiente');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Det_Producto')
    ALTER TABLE [dbo].[Detalle_Pedido] WITH CHECK ADD CONSTRAINT [FK_Det_Producto] FOREIGN KEY([id_producto])
    REFERENCES [dbo].[Productos] ([id_producto]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Det_Pedido')
    ALTER TABLE [dbo].[Detalle_Pedido] WITH CHECK ADD CONSTRAINT [FK_Det_Pedido] FOREIGN KEY([id_pedido])
    REFERENCES [dbo].[Pedidos] ([id_pedido]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Detalle_pedido' AND object_id = OBJECT_ID('Detalle_Pedido'))
    CREATE  NONCLUSTERED INDEX [IX_Detalle_pedido] ON [dbo].[Detalle_Pedido] (id_pedido);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Detalle_producto' AND object_id = OBJECT_ID('Detalle_Pedido'))
    CREATE  NONCLUSTERED INDEX [IX_Detalle_producto] ON [dbo].[Detalle_Pedido] (id_producto);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_DetVen_estado')
    ALTER TABLE [dbo].[Detalle_Venta] ADD CONSTRAINT [DF_DetVen_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DetVen_Producto')
    ALTER TABLE [dbo].[Detalle_Venta] WITH CHECK ADD CONSTRAINT [FK_DetVen_Producto] FOREIGN KEY([id_producto])
    REFERENCES [dbo].[Productos] ([id_producto]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_DetVen_Venta')
    ALTER TABLE [dbo].[Detalle_Venta] WITH CHECK ADD CONSTRAINT [FK_DetVen_Venta] FOREIGN KEY([id_venta])
    REFERENCES [dbo].[Ventas] ([id_venta]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Detalle_venta' AND object_id = OBJECT_ID('Detalle_Venta'))
    CREATE  NONCLUSTERED INDEX [IX_Detalle_venta] ON [dbo].[Detalle_Venta] (id_venta);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Egr_metodo')
    ALTER TABLE [dbo].[Egresos_Caja] ADD CONSTRAINT [DF_Egr_metodo] DEFAULT ('efectivo') FOR [metodo_pago];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Egr_fecha')
    ALTER TABLE [dbo].[Egresos_Caja] ADD CONSTRAINT [DF_Egr_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_hora];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Egr_estado')
    ALTER TABLE [dbo].[Egresos_Caja] ADD CONSTRAINT [DF_Egr_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Egr_metodo')
    ALTER TABLE [dbo].[Egresos_Caja] ADD CONSTRAINT [CK_Egr_metodo] CHECK ([metodo_pago]='mixto' OR [metodo_pago]='tarjeta' OR [metodo_pago]='sinpe' OR [metodo_pago]='efectivo');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Egr_monto')
    ALTER TABLE [dbo].[Egresos_Caja] ADD CONSTRAINT [CK_Egr_monto] CHECK ([monto]>(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Egr_Apertura')
    ALTER TABLE [dbo].[Egresos_Caja] WITH CHECK ADD CONSTRAINT [FK_Egr_Apertura] FOREIGN KEY([id_apertura])
    REFERENCES [dbo].[Apertura_Caja] ([id_apertura]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Egr_Usuario')
    ALTER TABLE [dbo].[Egresos_Caja] WITH CHECK ADD CONSTRAINT [FK_Egr_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Emp_vac')
    ALTER TABLE [dbo].[Empleados] ADD CONSTRAINT [DF_Emp_vac] DEFAULT ((0)) FOR [dias_vacaciones_disponibles];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Emp_ingreso')
    ALTER TABLE [dbo].[Empleados] ADD CONSTRAINT [DF_Emp_ingreso] DEFAULT (CONVERT([date],sysutcdatetime())) FOR [fecha_ingreso];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Emp_estado')
    ALTER TABLE [dbo].[Empleados] ADD CONSTRAINT [DF_Emp_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Emp_salario')
    ALTER TABLE [dbo].[Empleados] ADD CONSTRAINT [CK_Emp_salario] CHECK ([salario_hora]>=(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Emp_Usuario')
    ALTER TABLE [dbo].[Empleados] WITH CHECK ADD CONSTRAINT [FK_Emp_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Emp_cedula')
    ALTER TABLE [dbo].[Empleados] ADD CONSTRAINT [UQ_Emp_cedula] UNIQUE (cedula);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Emp_usuario')
    ALTER TABLE [dbo].[Empleados] ADD CONSTRAINT [UQ_Emp_usuario] UNIQUE (id_usuario);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_EnvPDV_prioridad')
    ALTER TABLE [dbo].[Envios_PDV] ADD CONSTRAINT [DF_EnvPDV_prioridad] DEFAULT ('Normal') FOR [prioridad];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_EnvPDV_fecha')
    ALTER TABLE [dbo].[Envios_PDV] ADD CONSTRAINT [DF_EnvPDV_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_hora_envio];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_EnvPDV_destino')
    ALTER TABLE [dbo].[Envios_PDV] ADD CONSTRAINT [CK_EnvPDV_destino] CHECK ([destino]='Caja' OR [destino]='Bar' OR [destino]='Cocina');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_EnvPDV_prioridad')
    ALTER TABLE [dbo].[Envios_PDV] ADD CONSTRAINT [CK_EnvPDV_prioridad] CHECK ([prioridad]='Urgente' OR [prioridad]='Normal');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_EnvPDV_estado')
    ALTER TABLE [dbo].[Envios_PDV] ADD CONSTRAINT [CK_EnvPDV_estado] CHECK ([estado_envio]='Reenviado' OR [estado_envio]='Fallido' OR [estado_envio]='Enviado' OR [estado_envio]='Pendiente');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EnvPDV_Venta')
    ALTER TABLE [dbo].[Envios_PDV] WITH CHECK ADD CONSTRAINT [FK_EnvPDV_Venta] FOREIGN KEY([id_venta])
    REFERENCES [dbo].[Ventas] ([id_venta]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_EnvPDV_Usuario')
    ALTER TABLE [dbo].[Envios_PDV] WITH CHECK ADD CONSTRAINT [FK_EnvPDV_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EnviosPDV_venta' AND object_id = OBJECT_ID('Envios_PDV'))
    CREATE  NONCLUSTERED INDEX [IX_EnviosPDV_venta] ON [dbo].[Envios_PDV] (id_venta, fecha_hora_envio);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Historial__fecha__32767D0B')
    ALTER TABLE [dbo].[Historial_Estados_Pedido] ADD CONSTRAINT [DF__Historial__fecha__32767D0B] DEFAULT (getdate()) FOR [fecha_hora_cambio];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Historial__estad__336AA144')
    ALTER TABLE [dbo].[Historial_Estados_Pedido] ADD CONSTRAINT [DF__Historial__estad__336AA144] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Historial_Estados_Pedido_Pedidos')
    ALTER TABLE [dbo].[Historial_Estados_Pedido] WITH CHECK ADD CONSTRAINT [FK_Historial_Estados_Pedido_Pedidos] FOREIGN KEY([id_pedido])
    REFERENCES [dbo].[Pedidos] ([id_pedido]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_HE_factor')
    ALTER TABLE [dbo].[Horas_Extra] ADD CONSTRAINT [DF_HE_factor] DEFAULT ((1.50)) FOR [factor_pago];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_HE_fecha')
    ALTER TABLE [dbo].[Horas_Extra] ADD CONSTRAINT [DF_HE_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_registro];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_HE_estado')
    ALTER TABLE [dbo].[Horas_Extra] ADD CONSTRAINT [DF_HE_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_HE_horas')
    ALTER TABLE [dbo].[Horas_Extra] ADD CONSTRAINT [CK_HE_horas] CHECK ([cantidad_horas]>(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_HE_Asistencia')
    ALTER TABLE [dbo].[Horas_Extra] WITH CHECK ADD CONSTRAINT [FK_HE_Asistencia] FOREIGN KEY([id_asistencia])
    REFERENCES [dbo].[Asistencia] ([id_asistencia]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Insumos_stock_min')
    ALTER TABLE [dbo].[Insumos] ADD CONSTRAINT [DF_Insumos_stock_min] DEFAULT ((0)) FOR [stock_minimo];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Insumos_stock_act')
    ALTER TABLE [dbo].[Insumos] ADD CONSTRAINT [DF_Insumos_stock_act] DEFAULT ((0)) FOR [stock_actual];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Insumos_costo')
    ALTER TABLE [dbo].[Insumos] ADD CONSTRAINT [DF_Insumos_costo] DEFAULT ((0)) FOR [costo_unitario];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Insumos_estado')
    ALTER TABLE [dbo].[Insumos] ADD CONSTRAINT [DF_Insumos_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Insumos_stock_min')
    ALTER TABLE [dbo].[Insumos] ADD CONSTRAINT [CK_Insumos_stock_min] CHECK ([stock_minimo]>=(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Insumos_stock_act')
    ALTER TABLE [dbo].[Insumos] ADD CONSTRAINT [CK_Insumos_stock_act] CHECK ([stock_actual]>=(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Insumos_costo')
    ALTER TABLE [dbo].[Insumos] ADD CONSTRAINT [CK_Insumos_costo] CHECK ([costo_unitario]>=(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Insumos_Categoria')
    ALTER TABLE [dbo].[Insumos] WITH CHECK ADD CONSTRAINT [FK_Insumos_Categoria] FOREIGN KEY([id_categoria])
    REFERENCES [dbo].[Categorias_Insumo] ([id_categoria]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Insumos_stock' AND object_id = OBJECT_ID('Insumos'))
    CREATE  NONCLUSTERED INDEX [IX_Insumos_stock] ON [dbo].[Insumos] (stock_actual, stock_minimo);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_InsProv_fecha')
    ALTER TABLE [dbo].[Insumos_Proveedores] ADD CONSTRAINT [DF_InsProv_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_asoc];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_InsProv_Proveedor')
    ALTER TABLE [dbo].[Insumos_Proveedores] WITH CHECK ADD CONSTRAINT [FK_InsProv_Proveedor] FOREIGN KEY([id_proveedor])
    REFERENCES [dbo].[Proveedores] ([id_proveedor]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_InsProv_Insumo')
    ALTER TABLE [dbo].[Insumos_Proveedores] WITH CHECK ADD CONSTRAINT [FK_InsProv_Insumo] FOREIGN KEY([id_insumo])
    REFERENCES [dbo].[Insumos] ([id_insumo]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Mesas_estado_mesa')
    ALTER TABLE [dbo].[Mesas] ADD CONSTRAINT [DF_Mesas_estado_mesa] DEFAULT ('disponible') FOR [estado_mesa];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Mesas_estado')
    ALTER TABLE [dbo].[Mesas] ADD CONSTRAINT [DF_Mesas_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Mesas_capacidad')
    ALTER TABLE [dbo].[Mesas] ADD CONSTRAINT [CK_Mesas_capacidad] CHECK ([capacidad]>(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Mesas_estado_mesa')
    ALTER TABLE [dbo].[Mesas] ADD CONSTRAINT [CK_Mesas_estado_mesa] CHECK ([estado_mesa]='inactiva' OR [estado_mesa]='sucia' OR [estado_mesa]='reservada' OR [estado_mesa]='ocupada' OR [estado_mesa]='disponible');
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Mesas_numero')
    ALTER TABLE [dbo].[Mesas] ADD CONSTRAINT [UQ_Mesas_numero] UNIQUE (numero_mesa);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__horas__6319B466')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__horas__6319B466] DEFAULT ((0)) FOR [horas_trabajadas];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__horas__640DD89F')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__horas__640DD89F] DEFAULT ((0)) FOR [horas_extra];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__vacac__6501FCD8')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__vacac__6501FCD8] DEFAULT ((0)) FOR [vacaciones_pagadas];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__horas__65F62111')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__horas__65F62111] DEFAULT ((8)) FOR [horas_por_dia_vacacion];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__monto__66EA454A')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__monto__66EA454A] DEFAULT ((0)) FOR [monto_vacaciones];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__dias___67DE6983')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__dias___67DE6983] DEFAULT ((0)) FOR [dias_trabajados];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__dias___68D28DBC')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__dias___68D28DBC] DEFAULT ((0)) FOR [dias_ausentes];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__valor__69C6B1F5')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__valor__69C6B1F5] DEFAULT ((0)) FOR [valor_hora];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__salar__6ABAD62E')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__salar__6ABAD62E] DEFAULT ((0)) FOR [salario_base];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__monto__6BAEFA67')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__monto__6BAEFA67] DEFAULT ((0)) FOR [monto_horas_extra];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__bonif__6CA31EA0')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__bonif__6CA31EA0] DEFAULT ((0)) FOR [bonificaciones];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__deduc__6D9742D9')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__deduc__6D9742D9] DEFAULT ((0)) FOR [deducciones];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__salar__6E8B6712')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__salar__6E8B6712] DEFAULT ((0)) FOR [salario_bruto];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__salar__6F7F8B4B')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__salar__6F7F8B4B] DEFAULT ((0)) FOR [salario_neto];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__estad__7073AF84')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__estad__7073AF84] DEFAULT ('pendiente') FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__creat__7167D3BD')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__creat__7167D3BD] DEFAULT (getdate()) FOR [created_at];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF__Nomina_Me__updat__725BF7F6')
    ALTER TABLE [dbo].[Nomina_Mensual] ADD CONSTRAINT [DF__Nomina_Me__updat__725BF7F6] DEFAULT (getdate()) FOR [updated_at];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_nomina_mensual_empleado')
    ALTER TABLE [dbo].[Nomina_Mensual] WITH CHECK ADD CONSTRAINT [FK_nomina_mensual_empleado] FOREIGN KEY([empleado_id])
    REFERENCES [dbo].[Empleados] ([id_empleado]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_NC_fecha')
    ALTER TABLE [dbo].[Notas_Credito] ADD CONSTRAINT [DF_NC_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_hora];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_NC_estado')
    ALTER TABLE [dbo].[Notas_Credito] ADD CONSTRAINT [DF_NC_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_NC_monto')
    ALTER TABLE [dbo].[Notas_Credito] ADD CONSTRAINT [CK_NC_monto] CHECK ([monto]>(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NC_Venta')
    ALTER TABLE [dbo].[Notas_Credito] WITH CHECK ADD CONSTRAINT [FK_NC_Venta] FOREIGN KEY([id_venta])
    REFERENCES [dbo].[Ventas] ([id_venta]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_NC_Usuario')
    ALTER TABLE [dbo].[Notas_Credito] WITH CHECK ADD CONSTRAINT [FK_NC_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_PH_fecha')
    ALTER TABLE [dbo].[PasswordHistorial] ADD CONSTRAINT [DF_PH_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_cambio];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_PH_metodo')
    ALTER TABLE [dbo].[PasswordHistorial] ADD CONSTRAINT [DF_PH_metodo] DEFAULT ('MANUAL') FOR [metodo_cambio];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_PH_metodo')
    ALTER TABLE [dbo].[PasswordHistorial] ADD CONSTRAINT [CK_PH_metodo] CHECK ([metodo_cambio]='PRIMER_INGRESO' OR [metodo_cambio]='OBLIGATORIO' OR [metodo_cambio]='MANUAL');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PH_Usuario')
    ALTER TABLE [dbo].[PasswordHistorial] WITH CHECK ADD CONSTRAINT [FK_PH_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PH_usuario_fecha' AND object_id = OBJECT_ID('PasswordHistorial'))
    CREATE  NONCLUSTERED INDEX [IX_PH_usuario_fecha] ON [dbo].[PasswordHistorial] (id_usuario, fecha_cambio);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ped_tipo')
    ALTER TABLE [dbo].[Pedidos] ADD CONSTRAINT [DF_Ped_tipo] DEFAULT ('mesa') FOR [tipo_servicio];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ped_estado')
    ALTER TABLE [dbo].[Pedidos] ADD CONSTRAINT [DF_Ped_estado] DEFAULT ('abierto') FOR [estado_pedido];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ped_fecha')
    ALTER TABLE [dbo].[Pedidos] ADD CONSTRAINT [DF_Ped_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_hora];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ped_activo')
    ALTER TABLE [dbo].[Pedidos] ADD CONSTRAINT [DF_Ped_activo] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Ped_tipo')
    ALTER TABLE [dbo].[Pedidos] ADD CONSTRAINT [CK_Ped_tipo] CHECK ([tipo_servicio]='Para llevar' OR [tipo_servicio]='Salon' OR [tipo_servicio]='mesa' OR [tipo_servicio]='para_llevar' OR [tipo_servicio]='delivery' OR [tipo_servicio]='rapida');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Ped_estado')
    ALTER TABLE [dbo].[Pedidos] ADD CONSTRAINT [CK_Ped_estado] CHECK ([estado_pedido]='finalizado' OR [estado_pedido]='abierto' OR [estado_pedido]='en_proceso' OR [estado_pedido]='listo' OR [estado_pedido]='entregado' OR [estado_pedido]='cancelado');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Ped_Empleado')
    ALTER TABLE [dbo].[Pedidos] WITH CHECK ADD CONSTRAINT [FK_Ped_Empleado] FOREIGN KEY([id_empleado])
    REFERENCES [dbo].[Empleados] ([id_empleado]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Ped_Mesa')
    ALTER TABLE [dbo].[Pedidos] WITH CHECK ADD CONSTRAINT [FK_Ped_Mesa] FOREIGN KEY([id_mesa])
    REFERENCES [dbo].[Mesas] ([id_mesa]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pedidos_fecha' AND object_id = OBJECT_ID('Pedidos'))
    CREATE  NONCLUSTERED INDEX [IX_Pedidos_fecha] ON [dbo].[Pedidos] (fecha_hora);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Prod_disp')
    ALTER TABLE [dbo].[Productos] ADD CONSTRAINT [DF_Prod_disp] DEFAULT ((1)) FOR [disponible];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Prod_estado')
    ALTER TABLE [dbo].[Productos] ADD CONSTRAINT [DF_Prod_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Prod_precio')
    ALTER TABLE [dbo].[Productos] ADD CONSTRAINT [CK_Prod_precio] CHECK ([precio_venta]>=(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Prod_Categoria')
    ALTER TABLE [dbo].[Productos] WITH CHECK ADD CONSTRAINT [FK_Prod_Categoria] FOREIGN KEY([id_categoria_prod])
    REFERENCES [dbo].[Categorias_Producto] ([id_categoria_prod]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Productos_categoria' AND object_id = OBJECT_ID('Productos'))
    CREATE  NONCLUSTERED INDEX [IX_Productos_categoria] ON [dbo].[Productos] (id_categoria_prod);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Productos_disponible' AND object_id = OBJECT_ID('Productos'))
    CREATE  NONCLUSTERED INDEX [IX_Productos_disponible] ON [dbo].[Productos] (disponible) WHERE ([disponible]=(1));
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Proveedores_estado')
    ALTER TABLE [dbo].[Proveedores] ADD CONSTRAINT [DF_Proveedores_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Prov_cedula')
    ALTER TABLE [dbo].[Proveedores] ADD CONSTRAINT [UQ_Prov_cedula] UNIQUE (cedula_juridica);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Receta_estado')
    ALTER TABLE [dbo].[Receta] ADD CONSTRAINT [DF_Receta_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Receta_Producto')
    ALTER TABLE [dbo].[Receta] WITH CHECK ADD CONSTRAINT [FK_Receta_Producto] FOREIGN KEY([id_producto])
    REFERENCES [dbo].[Productos] ([id_producto]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Receta_Producto')
    ALTER TABLE [dbo].[Receta] ADD CONSTRAINT [UQ_Receta_Producto] UNIQUE (id_producto);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_RI_cant')
    ALTER TABLE [dbo].[RecetaInsumo] ADD CONSTRAINT [CK_RI_cant] CHECK ([cantidad_usar]>(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RI_Receta')
    ALTER TABLE [dbo].[RecetaInsumo] WITH CHECK ADD CONSTRAINT [FK_RI_Receta] FOREIGN KEY([id_receta])
    REFERENCES [dbo].[Receta] ([id_receta]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_RI_Insumo')
    ALTER TABLE [dbo].[RecetaInsumo] WITH CHECK ADD CONSTRAINT [FK_RI_Insumo] FOREIGN KEY([id_insumo])
    REFERENCES [dbo].[Insumos] ([id_insumo]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Rep_formato')
    ALTER TABLE [dbo].[Reportes_Generados] ADD CONSTRAINT [DF_Rep_formato] DEFAULT ('pdf') FOR [formato_salida];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Rep_fecha')
    ALTER TABLE [dbo].[Reportes_Generados] ADD CONSTRAINT [DF_Rep_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_generacion];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Rep_estado')
    ALTER TABLE [dbo].[Reportes_Generados] ADD CONSTRAINT [DF_Rep_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Rep_tipo')
    ALTER TABLE [dbo].[Reportes_Generados] ADD CONSTRAINT [CK_Rep_tipo] CHECK ([tipo_reporte]='bitacora' OR [tipo_reporte]='cierre_turno' OR [tipo_reporte]='egresos' OR [tipo_reporte]='inventario' OR [tipo_reporte]='desempenio_meseros' OR [tipo_reporte]='ingresos_metodo_pago' OR [tipo_reporte]='productos_mas_vendidos' OR [tipo_reporte]='ventas');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Rep_formato')
    ALTER TABLE [dbo].[Reportes_Generados] ADD CONSTRAINT [CK_Rep_formato] CHECK ([formato_salida]='csv' OR [formato_salida]='excel' OR [formato_salida]='pdf');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Rep_Usuario')
    ALTER TABLE [dbo].[Reportes_Generados] WITH CHECK ADD CONSTRAINT [FK_Rep_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Roles_estado')
    ALTER TABLE [dbo].[Roles] ADD CONSTRAINT [DF_Roles_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Roles_nombre')
    ALTER TABLE [dbo].[Roles] ADD CONSTRAINT [UQ_Roles_nombre] UNIQUE (nombre_rol);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ses_estado')
    ALTER TABLE [dbo].[Sesiones] ADD CONSTRAINT [DF_Ses_estado] DEFAULT ('ACTIVA') FOR [estado_sesion];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ses_inicio')
    ALTER TABLE [dbo].[Sesiones] ADD CONSTRAINT [DF_Ses_inicio] DEFAULT (sysutcdatetime()) FOR [fecha_hora_inicio];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ses_ult_act')
    ALTER TABLE [dbo].[Sesiones] ADD CONSTRAINT [DF_Ses_ult_act] DEFAULT (sysutcdatetime()) FOR [fecha_hora_ultima_act];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Ses_estado')
    ALTER TABLE [dbo].[Sesiones] ADD CONSTRAINT [CK_Ses_estado] CHECK ([estado_sesion]='EXPIRADA' OR [estado_sesion]='CERRADA' OR [estado_sesion]='INACTIVA' OR [estado_sesion]='ACTIVA');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Ses_Usuario')
    ALTER TABLE [dbo].[Sesiones] WITH CHECK ADD CONSTRAINT [FK_Ses_Usuario] FOREIGN KEY([id_usuario])
    REFERENCES [dbo].[Usuarios] ([id_usuario]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Ses_fecha_ult_act' AND object_id = OBJECT_ID('Sesiones'))
    CREATE  NONCLUSTERED INDEX [IX_Ses_fecha_ult_act] ON [dbo].[Sesiones] (fecha_hora_ultima_act);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Ses_usuario_estado' AND object_id = OBJECT_ID('Sesiones'))
    CREATE  NONCLUSTERED INDEX [IX_Ses_usuario_estado] ON [dbo].[Sesiones] (id_usuario, estado_sesion);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_SubDet_estado')
    ALTER TABLE [dbo].[Subcuenta_Detalle_Pedido] ADD CONSTRAINT [DF_SubDet_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SubDet_Detalle')
    ALTER TABLE [dbo].[Subcuenta_Detalle_Pedido] WITH CHECK ADD CONSTRAINT [FK_SubDet_Detalle] FOREIGN KEY([id_detalle_pedido])
    REFERENCES [dbo].[Detalle_Pedido] ([id_detalle]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_SubDet_Subcuenta')
    ALTER TABLE [dbo].[Subcuenta_Detalle_Pedido] WITH CHECK ADD CONSTRAINT [FK_SubDet_Subcuenta] FOREIGN KEY([id_subcuenta])
    REFERENCES [dbo].[Subcuentas_Pedido] ([id_subcuenta]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_SubDet_subcuenta' AND object_id = OBJECT_ID('Subcuenta_Detalle_Pedido'))
    CREATE  NONCLUSTERED INDEX [IX_SubDet_subcuenta] ON [dbo].[Subcuenta_Detalle_Pedido] (id_subcuenta);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_SubCuentas_estado')
    ALTER TABLE [dbo].[Subcuentas_Pedido] ADD CONSTRAINT [DF_SubCuentas_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Sub_Pedido')
    ALTER TABLE [dbo].[Subcuentas_Pedido] WITH CHECK ADD CONSTRAINT [FK_Sub_Pedido] FOREIGN KEY([id_pedido])
    REFERENCES [dbo].[Pedidos] ([id_pedido]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Turnos_estado')
    ALTER TABLE [dbo].[Turnos_Trabajo] ADD CONSTRAINT [DF_Turnos_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Turnos_horas')
    ALTER TABLE [dbo].[Turnos_Trabajo] ADD CONSTRAINT [CK_Turnos_horas] CHECK ([hora_fin]>[hora_inicio]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Turnos_Empleado')
    ALTER TABLE [dbo].[Turnos_Trabajo] WITH CHECK ADD CONSTRAINT [FK_Turnos_Empleado] FOREIGN KEY([id_empleado])
    REFERENCES [dbo].[Empleados] ([id_empleado]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Turnos_fecha' AND object_id = OBJECT_ID('Turnos_Trabajo'))
    CREATE  NONCLUSTERED INDEX [IX_Turnos_fecha] ON [dbo].[Turnos_Trabajo] (fecha_turno);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Turnos_emp_dia')
    ALTER TABLE [dbo].[Turnos_Trabajo] ADD CONSTRAINT [UQ_Turnos_emp_dia] UNIQUE (id_empleado, fecha_turno);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Usr_cambio_pwd')
    ALTER TABLE [dbo].[Usuarios] ADD CONSTRAINT [DF_Usr_cambio_pwd] DEFAULT ((0)) FOR [cambio_password_requerido];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Usr_intentos')
    ALTER TABLE [dbo].[Usuarios] ADD CONSTRAINT [DF_Usr_intentos] DEFAULT ((0)) FOR [intentos_fallidos];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Usr_bloqueado')
    ALTER TABLE [dbo].[Usuarios] ADD CONSTRAINT [DF_Usr_bloqueado] DEFAULT ((0)) FOR [bloqueado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Usuarios_estado')
    ALTER TABLE [dbo].[Usuarios] ADD CONSTRAINT [DF_Usuarios_estado] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Usuarios_fecha')
    ALTER TABLE [dbo].[Usuarios] ADD CONSTRAINT [DF_Usuarios_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_creacion];
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Usuarios_Rol')
    ALTER TABLE [dbo].[Usuarios] WITH CHECK ADD CONSTRAINT [FK_Usuarios_Rol] FOREIGN KEY([id_rol])
    REFERENCES [dbo].[Roles] ([id_rol]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Usuarios_correo')
    ALTER TABLE [dbo].[Usuarios] ADD CONSTRAINT [UQ_Usuarios_correo] UNIQUE (correo);
GO

IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE name = 'UQ_Usuarios_nombre')
    ALTER TABLE [dbo].[Usuarios] ADD CONSTRAINT [UQ_Usuarios_nombre] UNIQUE (nombre_usuario);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Vac_estado')
    ALTER TABLE [dbo].[Vacaciones] ADD CONSTRAINT [DF_Vac_estado] DEFAULT ('pendiente') FOR [estado_solicitud];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Vac_fecha')
    ALTER TABLE [dbo].[Vacaciones] ADD CONSTRAINT [DF_Vac_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_solicitud];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Vac_activo')
    ALTER TABLE [dbo].[Vacaciones] ADD CONSTRAINT [DF_Vac_activo] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Vac_dias')
    ALTER TABLE [dbo].[Vacaciones] ADD CONSTRAINT [CK_Vac_dias] CHECK ([dias_solicitados]>(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Vac_fechas')
    ALTER TABLE [dbo].[Vacaciones] ADD CONSTRAINT [CK_Vac_fechas] CHECK ([fecha_fin]>=[fecha_inicio]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Vac_estado_sol')
    ALTER TABLE [dbo].[Vacaciones] ADD CONSTRAINT [CK_Vac_estado_sol] CHECK ([estado_solicitud]='rechazada' OR [estado_solicitud]='aprobada' OR [estado_solicitud]='pendiente');
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Vac_Empleado')
    ALTER TABLE [dbo].[Vacaciones] WITH CHECK ADD CONSTRAINT [FK_Vac_Empleado] FOREIGN KEY([id_empleado])
    REFERENCES [dbo].[Empleados] ([id_empleado]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Vac_Aprobador')
    ALTER TABLE [dbo].[Vacaciones] WITH CHECK ADD CONSTRAINT [FK_Vac_Aprobador] FOREIGN KEY([id_aprobador])
    REFERENCES [dbo].[Empleados] ([id_empleado]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Vacaciones_estado' AND object_id = OBJECT_ID('Vacaciones'))
    CREATE  NONCLUSTERED INDEX [IX_Vacaciones_estado] ON [dbo].[Vacaciones] (estado_solicitud, id_empleado);
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ven_tipo')
    ALTER TABLE [dbo].[Ventas] ADD CONSTRAINT [DF_Ven_tipo] DEFAULT ('normal') FOR [tipo_venta];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ven_recib')
    ALTER TABLE [dbo].[Ventas] ADD CONSTRAINT [DF_Ven_recib] DEFAULT ((0)) FOR [monto_recibido];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ven_vuelto')
    ALTER TABLE [dbo].[Ventas] ADD CONSTRAINT [DF_Ven_vuelto] DEFAULT ((0)) FOR [vuelto];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ven_estado')
    ALTER TABLE [dbo].[Ventas] ADD CONSTRAINT [DF_Ven_estado] DEFAULT ('completada') FOR [estado_venta];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ven_fecha')
    ALTER TABLE [dbo].[Ventas] ADD CONSTRAINT [DF_Ven_fecha] DEFAULT (sysutcdatetime()) FOR [fecha_hora];
GO

IF NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_Ven_activo')
    ALTER TABLE [dbo].[Ventas] ADD CONSTRAINT [DF_Ven_activo] DEFAULT ((1)) FOR [estado];
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Ven_tipo')
    ALTER TABLE [dbo].[Ventas] ADD CONSTRAINT [CK_Ven_tipo] CHECK ([tipo_venta]='rapida' OR [tipo_venta]='normal' OR [tipo_venta]='cortesia' OR [tipo_venta]='descuento');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Ven_estado_v')
    ALTER TABLE [dbo].[Ventas] ADD CONSTRAINT [CK_Ven_estado_v] CHECK ([estado_venta]='validada' OR [estado_venta]='en_revision' OR [estado_venta]='abierta' OR [estado_venta]='completada' OR [estado_venta]='anulada' OR [estado_venta]='pendiente');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Ven_total')
    ALTER TABLE [dbo].[Ventas] ADD CONSTRAINT [CK_Ven_total] CHECK ([total_cobrado]>=(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Ven_metodo')
    ALTER TABLE [dbo].[Ventas] ADD CONSTRAINT [CK_Ven_metodo] CHECK ([metodo_pago]='mixto' OR [metodo_pago]='sinpe' OR [metodo_pago]='tarjeta' OR [metodo_pago]='efectivo');
GO

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Ven_vuelto')
    ALTER TABLE [dbo].[Ventas] ADD CONSTRAINT [CK_Ven_vuelto] CHECK ([vuelto]>=(0));
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Ven_Empleado')
    ALTER TABLE [dbo].[Ventas] WITH CHECK ADD CONSTRAINT [FK_Ven_Empleado] FOREIGN KEY([id_empleado])
    REFERENCES [dbo].[Empleados] ([id_empleado]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Ven_Pedido')
    ALTER TABLE [dbo].[Ventas] WITH CHECK ADD CONSTRAINT [FK_Ven_Pedido] FOREIGN KEY([id_pedido])
    REFERENCES [dbo].[Pedidos] ([id_pedido]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Ven_Subcuenta')
    ALTER TABLE [dbo].[Ventas] WITH CHECK ADD CONSTRAINT [FK_Ven_Subcuenta] FOREIGN KEY([id_subcuenta])
    REFERENCES [dbo].[Subcuentas_Pedido] ([id_subcuenta]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Ven_Apertura')
    ALTER TABLE [dbo].[Ventas] WITH CHECK ADD CONSTRAINT [FK_Ven_Apertura] FOREIGN KEY([id_apertura])
    REFERENCES [dbo].[Apertura_Caja] ([id_apertura]);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Ventas_apertura' AND object_id = OBJECT_ID('Ventas'))
    CREATE  NONCLUSTERED INDEX [IX_Ventas_apertura] ON [dbo].[Ventas] (id_apertura);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Ventas_fecha' AND object_id = OBJECT_ID('Ventas'))
    CREATE  NONCLUSTERED INDEX [IX_Ventas_fecha] ON [dbo].[Ventas] (fecha_hora);
GO

-- ======================================================================
-- VISTAS
-- ======================================================================

CREATE OR ALTER VIEW dbo.v_cierres_caja AS
SELECT
    cc.id_cierre, cc.id_cajero, cc.id_apertura, ac.fecha_apertura,
    cc.fecha_cierre, cc.monto_apertura, cc.total_efectivo,
    cc.total_sinpe, cc.total_tarjeta, cc.total_egresos,
    cc.saldo_esperado, cc.saldo_real,
    cc.saldo_real - cc.saldo_esperado AS monto_diferencia,
    cc.descuadre, cc.estado
FROM dbo.Cierres_Caja cc
JOIN dbo.Apertura_Caja ac ON ac.id_apertura = cc.id_apertura;
GO

CREATE OR ALTER VIEW dbo.v_desempenio_meseros AS
SELECT
    e.id_empleado,
    e.nombre + ' ' + e.apellidos                                        AS nombre_completo,
    COUNT(DISTINCT ped.id_pedido)                                        AS total_pedidos,
    COUNT(DISTINCT ped.id_mesa)                                          AS mesas_atendidas,
    SUM(dp.cantidad * dp.precio_unitario)                                AS monto_total_vendido,
    AVG(CAST(DATEDIFF(MINUTE, ped.fecha_hora, v.fecha_hora) AS FLOAT))  AS tiempo_promedio_minutos
FROM dbo.Empleados e
JOIN dbo.Pedidos        ped ON ped.id_empleado = e.id_empleado AND ped.estado = 1
JOIN dbo.Detalle_Pedido dp  ON dp.id_pedido    = ped.id_pedido AND dp.estado  = 1
LEFT JOIN dbo.Ventas    v   ON v.id_pedido     = ped.id_pedido
GROUP BY e.id_empleado, e.nombre, e.apellidos;
GO

CREATE OR ALTER VIEW dbo.v_productos_mas_vendidos AS
SELECT
    p.id_producto, p.nombre_producto, cp.nombre_categoria,
    SUM(dp.cantidad)                      AS total_unidades_vendidas,
    SUM(dp.cantidad * dp.precio_unitario) AS total_ingresos,
    COUNT(DISTINCT dp.id_pedido)          AS total_pedidos
FROM dbo.Detalle_Pedido dp
JOIN dbo.Productos          p  ON p.id_producto        = dp.id_producto
JOIN dbo.Categorias_Producto cp ON cp.id_categoria_prod = p.id_categoria_prod
WHERE dp.estado = 1 AND dp.estado_item <> 'cancelado'
GROUP BY p.id_producto, p.nombre_producto, cp.nombre_categoria;
GO

CREATE OR ALTER VIEW dbo.v_stock_bajo AS
SELECT
    i.id_insumo, i.nombre_insumo, c.nombre_categoria,
    i.unidad_medida, i.stock_minimo, i.stock_actual,
    i.stock_minimo - i.stock_actual AS unidades_faltantes
FROM dbo.Insumos i
JOIN dbo.Categorias_Insumo c ON c.id_categoria = i.id_categoria
WHERE i.stock_actual < i.stock_minimo AND i.estado = 1;
GO

CREATE OR ALTER VIEW dbo.v_ventas_totales AS
SELECT
    v.id_venta, v.id_pedido, v.id_empleado, v.id_apertura,
    v.tipo_venta, v.metodo_pago, v.estado_venta, v.fecha_hora,
    v.total_cobrado, v.monto_recibido, v.vuelto,
    SUM(dp.cantidad * dp.precio_unitario)        AS subtotal_calculado,
    SUM(dp.cantidad * dp.precio_unitario) * 0.13 AS iva_calculado,
    SUM(dp.cantidad * dp.precio_unitario) * 1.13 AS total_con_iva
FROM dbo.Ventas v
LEFT JOIN dbo.Detalle_Pedido dp ON dp.id_pedido = v.id_pedido AND dp.estado = 1
GROUP BY v.id_venta, v.id_pedido, v.id_empleado, v.id_apertura,
         v.tipo_venta, v.metodo_pago, v.estado_venta,
         v.fecha_hora, v.total_cobrado, v.monto_recibido, v.vuelto;
GO


-- ======================================================================
-- TRIGGERS: EXCLUSION MUTUA VACACIONES / TURNOS
-- ======================================================================
-- ======================================================================
-- TRIGGERS
-- ======================================================================

IF OBJECT_ID('dbo.TRG_Check_Vacaciones_Turnos', 'TR') IS NOT NULL DROP TRIGGER dbo.TRG_Check_Vacaciones_Turnos;
GO

-- Trigger para bloquear el ingreso de vacaciones que colisionen con turnos ya creados
-- Procedimiento de exclusion mutua para las vacaciones
CREATE TRIGGER dbo.TRG_Check_Vacaciones_Turnos
ON dbo.Vacaciones
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 
        FROM inserted i
        INNER JOIN dbo.Turnos_Trabajo t 
            ON i.id_empleado = t.id_empleado
        WHERE i.estado_solicitud = 'aprobada' 
          AND t.estado = 1
          AND t.fecha_turno BETWEEN i.fecha_inicio AND i.fecha_fin
    )
    BEGIN
        RAISERROR ('No se puede aprobar la vacacion. El empleado cuenta con turnos de trabajo programados en el rango de fechas solicitado.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;

GO

IF OBJECT_ID('dbo.TRG_Check_Turnos_Vacaciones', 'TR') IS NOT NULL DROP TRIGGER dbo.TRG_Check_Turnos_Vacaciones;
GO

-- Trigger para bloquear el ingreso de turnos que colisionen con vacaciones aprobadas
-- Procedimiento de exclusion mutua para los turnos
CREATE TRIGGER dbo.TRG_Check_Turnos_Vacaciones
ON dbo.Turnos_Trabajo
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 
        FROM inserted i
        INNER JOIN dbo.Vacaciones v 
            ON i.id_empleado = v.id_empleado
        WHERE v.estado_solicitud = 'aprobada' 
          AND v.estado = 1
          AND i.estado = 1
          AND i.fecha_turno BETWEEN v.fecha_inicio AND v.fecha_fin
    )
    BEGIN
        RAISERROR ('No se puede programar el turno. El empleado se encuentra de vacaciones en la fecha especificada.', 16, 1);
        ROLLBACK TRANSACTION;
    END
END;

GO

-- ======================================================================
-- TRIGGERS: INMUTABILIDAD DE BITACORAS
-- ======================================================================

-- TRIGGER: inmutabilidad de la bitacora financiera
IF OBJECT_ID('trg_Bitacora_Financiera_Inmutable', 'TR') IS NULL
BEGIN
    EXEC('CREATE TRIGGER [dbo].[trg_Bitacora_Financiera_Inmutable]
    ON [dbo].[Bitacora_Financiera]
    AFTER UPDATE, DELETE
    AS
    BEGIN
        SET NOCOUNT ON;
        RAISERROR(''Los registros de la bitacora financiera son inmutables: no se permite modificarlos ni eliminarlos.'', 16, 1);
        ROLLBACK TRANSACTION;
    END');
END
GO

-- TRIGGER: inmutabilidad de la bitacora del punto de venta
IF OBJECT_ID('trg_Bitacora_PDV_Inmutable', 'TR') IS NULL
BEGIN
    EXEC('CREATE TRIGGER [dbo].[trg_Bitacora_PDV_Inmutable] ON [dbo].[Bitacora_PDV] AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    RAISERROR(''Los registros de la bitacora del punto de venta son inmutables: no se permite modificar ni eliminar.'', 16, 1);
    ROLLBACK TRANSACTION;
END');
END
GO

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

-- TRIGGER: inmutabilidad de la bitacora de reportes (RPT 008)
IF OBJECT_ID('trg_Bitacora_Reportes_Inmutable', 'TR') IS NULL
BEGIN
    EXEC('CREATE TRIGGER trg_Bitacora_Reportes_Inmutable ON Bitacora_Reportes
AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    RAISERROR(''Los registros de la bitacora de reportes son inmutables: no se permite modificar ni eliminar.'', 16, 1);
    ROLLBACK TRANSACTION;
END');
END
GO

