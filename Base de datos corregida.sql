USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'COLIBRI')
BEGIN
    CREATE DATABASE COLIBRI
        COLLATE Modern_Spanish_CI_AI;
    PRINT 'Base de datos COLIBRI creada.';
END
GO

USE COLIBRI;
GO

-- Limpieza de triggers existentes para evitar errores de recreacion
IF OBJECT_ID('dbo.TRG_Check_Vacaciones_Turnos', 'TR') IS NOT NULL DROP TRIGGER dbo.TRG_Check_Vacaciones_Turnos;
IF OBJECT_ID('dbo.TRG_Check_Turnos_Vacaciones', 'TR') IS NOT NULL DROP TRIGGER dbo.TRG_Check_Turnos_Vacaciones;

-- Limpieza de tablas existentes en orden inverso de dependencias
IF OBJECT_ID('dbo.Bitacora_Reportes',       'U') IS NOT NULL DROP TABLE dbo.Bitacora_Reportes;
IF OBJECT_ID('dbo.Reportes_Generados',      'U') IS NOT NULL DROP TABLE dbo.Reportes_Generados;
IF OBJECT_ID('dbo.Bitacora_Pedidos',        'U') IS NOT NULL DROP TABLE dbo.Bitacora_Pedidos;
IF OBJECT_ID('dbo.Bitacora_Acceso',         'U') IS NOT NULL DROP TABLE dbo.Bitacora_Acceso;
IF OBJECT_ID('dbo.Bitacora_Usuarios_RRHH',  'U') IS NOT NULL DROP TABLE dbo.Bitacora_Usuarios_RRHH;
IF OBJECT_ID('dbo.Bitacora_Financiera',     'U') IS NOT NULL DROP TABLE dbo.Bitacora_Financiera;
IF OBJECT_ID('dbo.Bitacora_Inventario',     'U') IS NOT NULL DROP TABLE dbo.Bitacora_Inventario;
IF OBJECT_ID('dbo.Notas_Credito',           'U') IS NOT NULL DROP TABLE dbo.Notas_Credito;
IF OBJECT_ID('dbo.Egresos_Caja',            'U') IS NOT NULL DROP TABLE dbo.Egresos_Caja;
IF OBJECT_ID('dbo.Cierres_Caja',            'U') IS NOT NULL DROP TABLE dbo.Cierres_Caja;
IF OBJECT_ID('dbo.Ventas',                  'U') IS NOT NULL DROP TABLE dbo.Ventas;
IF OBJECT_ID('dbo.Apertura_Caja',           'U') IS NOT NULL DROP TABLE dbo.Apertura_Caja;
IF OBJECT_ID('dbo.Cajas',                   'U') IS NOT NULL DROP TABLE dbo.Cajas;
IF OBJECT_ID('dbo.Subcuentas_Pedido',       'U') IS NOT NULL DROP TABLE dbo.Subcuentas_Pedido;
IF OBJECT_ID('dbo.Detalle_Pedido',          'U') IS NOT NULL DROP TABLE dbo.Detalle_Pedido;
IF OBJECT_ID('dbo.Pedidos',                 'U') IS NOT NULL DROP TABLE dbo.Pedidos;
IF OBJECT_ID('dbo.Horas_Extra',             'U') IS NOT NULL DROP TABLE dbo.Horas_Extra;
IF OBJECT_ID('dbo.Vacaciones',              'U') IS NOT NULL DROP TABLE dbo.Vacaciones;
IF OBJECT_ID('dbo.Asistencia',              'U') IS NOT NULL DROP TABLE dbo.Asistencia;
IF OBJECT_ID('dbo.Turnos_Trabajo',          'U') IS NOT NULL DROP TABLE dbo.Turnos_Trabajo;
IF OBJECT_ID('dbo.Empleados',               'U') IS NOT NULL DROP TABLE dbo.Empleados;
IF OBJECT_ID('dbo.Recetas',                 'U') IS NOT NULL DROP TABLE dbo.Recetas;
IF OBJECT_ID('dbo.Productos',               'U') IS NOT NULL DROP TABLE dbo.Productos;
IF OBJECT_ID('dbo.Categorias_Producto',     'U') IS NOT NULL DROP TABLE dbo.Categorias_Producto;
IF OBJECT_ID('dbo.Insumos_Proveedores',     'U') IS NOT NULL DROP TABLE dbo.Insumos_Proveedores;
IF OBJECT_ID('dbo.Insumos',                 'U') IS NOT NULL DROP TABLE dbo.Insumos;
IF OBJECT_ID('dbo.Categorias_Insumo',       'U') IS NOT NULL DROP TABLE dbo.Categorias_Insumo;
IF OBJECT_ID('dbo.Proveedores',             'U') IS NOT NULL DROP TABLE dbo.Proveedores;
IF OBJECT_ID('dbo.Mesas',                   'U') IS NOT NULL DROP TABLE dbo.Mesas;
IF OBJECT_ID('dbo.Usuarios',                'U') IS NOT NULL DROP TABLE dbo.Usuarios;
IF OBJECT_ID('dbo.Roles',                   'U') IS NOT NULL DROP TABLE dbo.Roles;
GO

-- Roles
CREATE TABLE dbo.Roles (
    id_rol       INT            NOT NULL IDENTITY(1,1),
    nombre_rol   NVARCHAR(60)   NOT NULL,
    descripcion  NVARCHAR(255)      NULL,
    estado       BIT            NOT NULL CONSTRAINT DF_Roles_estado DEFAULT 1,

    CONSTRAINT PK_Roles        PRIMARY KEY (id_rol),
    CONSTRAINT UQ_Roles_nombre UNIQUE      (nombre_rol)
);
GO

-- Usuarios
CREATE TABLE dbo.Usuarios (
    id_usuario                  INT           NOT NULL IDENTITY(1,1),
    id_rol                      INT           NOT NULL,
    nombre_usuario              NVARCHAR(80)  NOT NULL,
    correo                      NVARCHAR(150) NOT NULL,
    password_hash               NVARCHAR(255) NOT NULL,
    cambio_password_requerido   BIT           NOT NULL CONSTRAINT DF_Usr_cambio_pwd  DEFAULT 0,
    intentos_fallidos           TINYINT       NOT NULL CONSTRAINT DF_Usr_intentos    DEFAULT 0,
    bloqueado                   BIT           NOT NULL CONSTRAINT DF_Usr_bloqueado   DEFAULT 0,
    fecha_ultimo_acceso         DATETIME2         NULL,
    fecha_password              DATETIME2         NULL,
    estado                      BIT           NOT NULL CONSTRAINT DF_Usuarios_estado DEFAULT 1,
    fecha_creacion              DATETIME2     NOT NULL CONSTRAINT DF_Usuarios_fecha  DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Usuarios        PRIMARY KEY (id_usuario),
    CONSTRAINT UQ_Usuarios_correo UNIQUE      (correo),
    CONSTRAINT UQ_Usuarios_nombre UNIQUE      (nombre_usuario),
    CONSTRAINT FK_Usuarios_Rol    FOREIGN KEY (id_rol) REFERENCES dbo.Roles (id_rol)
);
GO

-- Mesas
CREATE TABLE dbo.Mesas (
    id_mesa      INT          NOT NULL IDENTITY(1,1),
    numero_mesa  NVARCHAR(10) NOT NULL,
    capacidad    TINYINT      NOT NULL CONSTRAINT CK_Mesas_capacidad   CHECK (capacidad > 0),
    estado_mesa  NVARCHAR(20) NOT NULL CONSTRAINT DF_Mesas_estado_mesa DEFAULT 'disponible',
    estado       BIT          NOT NULL CONSTRAINT DF_Mesas_estado      DEFAULT 1,

    CONSTRAINT PK_Mesas             PRIMARY KEY (id_mesa),
    CONSTRAINT UQ_Mesas_numero      UNIQUE      (numero_mesa),
    CONSTRAINT CK_Mesas_estado_mesa CHECK (estado_mesa IN ('disponible','ocupada','reservada','sucia','inactiva'))
);
GO

-- Proveedores
CREATE TABLE dbo.Proveedores (
    id_proveedor    INT           NOT NULL IDENTITY(1,1),
    cedula_juridica NVARCHAR(50)  NOT NULL,
    nombre_empresa  NVARCHAR(150) NOT NULL,
    contacto_nombre NVARCHAR(100)     NULL,
    telefono        NVARCHAR(20)      NULL,
    correo          NVARCHAR(150)     NULL,
    estado          BIT           NOT NULL CONSTRAINT DF_Proveedores_estado DEFAULT 1,

    CONSTRAINT PK_Proveedores PRIMARY KEY (id_proveedor),
    CONSTRAINT UQ_Prov_cedula UNIQUE (cedula_juridica)
);
GO

-- Categorias_Insumo
CREATE TABLE dbo.Categorias_Insumo (
    id_categoria      INT           NOT NULL IDENTITY(1,1),
    nombre_categoria  NVARCHAR(100) NOT NULL,
    estado            BIT           NOT NULL CONSTRAINT DF_CatIns_estado DEFAULT 1,

    CONSTRAINT PK_Categorias_Insumo PRIMARY KEY (id_categoria),
    CONSTRAINT UQ_CatIns_nombre     UNIQUE      (nombre_categoria)
);
GO

-- Insumos
CREATE TABLE dbo.Insumos (
    id_insumo      INT            NOT NULL IDENTITY(1,1),
    id_categoria   INT            NOT NULL,
    nombre_insumo  NVARCHAR(120)  NOT NULL,
    unidad_medida  NVARCHAR(30)   NOT NULL,
    stock_minimo   DECIMAL(10,2)  NOT NULL CONSTRAINT DF_Insumos_stock_min DEFAULT 0,
    stock_actual   DECIMAL(10,2)  NOT NULL CONSTRAINT DF_Insumos_stock_act DEFAULT 0,
    costo_unitario DECIMAL(10,2)  NOT NULL CONSTRAINT DF_Insumos_costo     DEFAULT 0,
    estado         BIT            NOT NULL CONSTRAINT DF_Insumos_estado    DEFAULT 1,

    CONSTRAINT PK_Insumos           PRIMARY KEY (id_insumo),
    CONSTRAINT FK_Insumos_Categoria FOREIGN KEY (id_categoria) REFERENCES dbo.Categorias_Insumo (id_categoria),
    CONSTRAINT CK_Insumos_stock_min CHECK (stock_minimo   >= 0),
    CONSTRAINT CK_Insumos_stock_act CHECK (stock_actual   >= 0),
    CONSTRAINT CK_Insumos_costo     CHECK (costo_unitario >= 0)
);
GO

-- Insumos_Proveedores
CREATE TABLE dbo.Insumos_Proveedores (
    id_insumo     INT NOT NULL,
    id_proveedor  INT NOT NULL,
    fecha_asoc    DATETIME2 NOT NULL CONSTRAINT DF_InsProv_fecha DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Insumos_Proveedores PRIMARY KEY (id_insumo, id_proveedor),
    CONSTRAINT FK_InsProv_Insumo FOREIGN KEY (id_insumo) REFERENCES dbo.Insumos (id_insumo),
    CONSTRAINT FK_InsProv_Proveedor FOREIGN KEY (id_proveedor) REFERENCES dbo.Proveedores (id_proveedor)
);
GO

-- Categorias_Producto
CREATE TABLE dbo.Categorias_Producto (
    id_categoria_prod  INT           NOT NULL IDENTITY(1,1),
    nombre_categoria   NVARCHAR(100) NOT NULL,
    estado             BIT           NOT NULL CONSTRAINT DF_CatProd_estado DEFAULT 1,

    CONSTRAINT PK_Categorias_Producto PRIMARY KEY (id_categoria_prod),
    CONSTRAINT UQ_CatProd_nombre      UNIQUE      (nombre_categoria)
);
GO

-- Productos
CREATE TABLE dbo.Productos (
    id_producto        INT            NOT NULL IDENTITY(1,1),
    id_categoria_prod  INT            NOT NULL,
    nombre_producto    NVARCHAR(120)  NOT NULL,
    descripcion        NVARCHAR(500)      NULL,
    precio_venta       DECIMAL(10,2)  NOT NULL CONSTRAINT CK_Prod_precio CHECK (precio_venta >= 0),
    disponible         BIT            NOT NULL CONSTRAINT DF_Prod_disp   DEFAULT 1,
    estado             BIT            NOT NULL CONSTRAINT DF_Prod_estado DEFAULT 1,

    CONSTRAINT PK_Productos      PRIMARY KEY (id_producto),
    CONSTRAINT FK_Prod_Categoria FOREIGN KEY (id_categoria_prod) REFERENCES dbo.Categorias_Producto (id_categoria_prod)
);
GO

-- Recetas
CREATE TABLE dbo.Recetas (
    id_producto   INT NOT NULL,
    id_insumo     INT NOT NULL,
    cantidad_usar DECIMAL(10,4) NOT NULL CONSTRAINT CK_Recetas_cant CHECK (cantidad_usar > 0),

    CONSTRAINT PK_Recetas PRIMARY KEY (id_producto, id_insumo),
    CONSTRAINT FK_Recetas_Producto FOREIGN KEY (id_producto) REFERENCES dbo.Productos (id_producto),
    CONSTRAINT FK_Recetas_Insumo FOREIGN KEY (id_insumo) REFERENCES dbo.Insumos (id_insumo)
);
GO

-- Empleados
CREATE TABLE dbo.Empleados (
    id_empleado                 INT           NOT NULL IDENTITY(1,1),
    id_usuario                  INT           NOT NULL,
    cedula                      NVARCHAR(20)  NOT NULL,
    nombre                      NVARCHAR(80)  NOT NULL,
    apellidos                   NVARCHAR(120) NOT NULL,
    telefono                    NVARCHAR(20)      NULL,
    correo_personal             NVARCHAR(150)     NULL,
    salario_hora                DECIMAL(10,2) NOT NULL CONSTRAINT CK_Emp_salario CHECK (salario_hora >= 0),
    dias_vacaciones_disponibles DECIMAL(6,2)  NOT NULL CONSTRAINT DF_Emp_vac     DEFAULT 0,
    fecha_ingreso               DATE          NOT NULL CONSTRAINT DF_Emp_ingreso  DEFAULT CAST(SYSUTCDATETIME() AS DATE),
    fecha_modificacion          DATETIME2         NULL,
    fecha_reactivacion          DATETIME2         NULL,
    motivo_inactivacion         NVARCHAR(300)     NULL,
    estado                      BIT           NOT NULL CONSTRAINT DF_Emp_estado   DEFAULT 1,

    CONSTRAINT PK_Empleados   PRIMARY KEY (id_empleado),
    CONSTRAINT UQ_Emp_cedula  UNIQUE      (cedula),
    CONSTRAINT UQ_Emp_usuario UNIQUE      (id_usuario),
    CONSTRAINT FK_Emp_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios (id_usuario)
);
GO

-- Turnos_Trabajo
CREATE TABLE dbo.Turnos_Trabajo (
    id_turno      INT           NOT NULL IDENTITY(1,1),
    id_empleado   INT           NOT NULL,
    fecha_turno   DATE          NOT NULL,
    hora_inicio   TIME          NOT NULL,
    hora_fin      TIME          NOT NULL,
    descripcion   NVARCHAR(200)     NULL,
    estado        BIT           NOT NULL CONSTRAINT DF_Turnos_estado DEFAULT 1,

    CONSTRAINT PK_Turnos_Trabajo  PRIMARY KEY (id_turno),
    CONSTRAINT FK_Turnos_Empleado FOREIGN KEY (id_empleado) REFERENCES dbo.Empleados (id_empleado),
    CONSTRAINT CK_Turnos_horas    CHECK (hora_fin > hora_inicio),
    CONSTRAINT UQ_Turnos_emp_dia  UNIQUE (id_empleado, fecha_turno)
);
GO

-- Asistencia
CREATE TABLE dbo.Asistencia (
    id_asistencia       INT           NOT NULL IDENTITY(1,1),
    id_empleado         INT           NOT NULL,
    id_turno            INT               NULL,
    fecha_hora_entrada  DATETIME2     NOT NULL CONSTRAINT DF_Asist_entrada DEFAULT SYSUTCDATETIME(),
    fecha_hora_salida   DATETIME2         NULL,
    observaciones       NVARCHAR(300)     NULL,
    estado              BIT           NOT NULL CONSTRAINT DF_Asist_estado  DEFAULT 1,

    CONSTRAINT PK_Asistencia     PRIMARY KEY (id_asistencia),
    CONSTRAINT FK_Asist_Empleado FOREIGN KEY (id_empleado) REFERENCES dbo.Empleados     (id_empleado),
    CONSTRAINT FK_Asist_Turno    FOREIGN KEY (id_turno)    REFERENCES dbo.Turnos_Trabajo (id_turno)
);
GO

-- Restriccion para evitar asistencias duplicadas al mismo turno/dia
CREATE UNIQUE INDEX UQ_Asistencia_Registro ON dbo.Asistencia (id_empleado, id_turno) WHERE id_turno IS NOT NULL AND estado = 1;
GO

-- Vacaciones
CREATE TABLE dbo.Vacaciones (
    id_vacacion      INT           NOT NULL IDENTITY(1,1),
    id_empleado      INT           NOT NULL,
    id_aprobador     INT               NULL,
    fecha_inicio     DATE          NOT NULL,
    fecha_fin        DATE          NOT NULL,
    dias_solicitados DECIMAL(5,1)  NOT NULL CONSTRAINT CK_Vac_dias CHECK (dias_solicitados > 0),
    estado_solicitud NVARCHAR(20)  NOT NULL CONSTRAINT DF_Vac_estado DEFAULT 'pendiente',
    motivo_rechazo   NVARCHAR(300)     NULL,
    fecha_solicitud  DATETIME2     NOT NULL CONSTRAINT DF_Vac_fecha  DEFAULT SYSUTCDATETIME(),
    estado           BIT           NOT NULL CONSTRAINT DF_Vac_activo DEFAULT 1,

    CONSTRAINT PK_Vacaciones     PRIMARY KEY (id_vacacion),
    CONSTRAINT FK_Vac_Empleado   FOREIGN KEY (id_empleado)  REFERENCES dbo.Empleados (id_empleado),
    CONSTRAINT FK_Vac_Aprobador  FOREIGN KEY (id_aprobador) REFERENCES dbo.Empleados (id_empleado),
    CONSTRAINT CK_Vac_fechas     CHECK (fecha_fin >= fecha_inicio),
    CONSTRAINT CK_Vac_estado_sol CHECK (estado_solicitud IN ('pendiente','aprobada','rechazada'))
);
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

-- Horas_Extra
CREATE TABLE dbo.Horas_Extra (
    id_hora_extra   INT            NOT NULL IDENTITY(1,1),
    id_asistencia   INT            NOT NULL,
    cantidad_horas  DECIMAL(5,2)   NOT NULL CONSTRAINT CK_HE_horas  CHECK (cantidad_horas > 0),
    factor_pago     DECIMAL(4,2)   NOT NULL CONSTRAINT DF_HE_factor DEFAULT 1.50,
    monto_calculado DECIMAL(12,2)      NULL,
    motivo_ajuste   NVARCHAR(300)      NULL,
    fecha_registro  DATETIME2      NOT NULL CONSTRAINT DF_HE_fecha  DEFAULT SYSUTCDATETIME(),
    estado          BIT            NOT NULL CONSTRAINT DF_HE_estado DEFAULT 1,

    CONSTRAINT PK_Horas_Extra   PRIMARY KEY (id_hora_extra),
    CONSTRAINT FK_HE_Asistencia FOREIGN KEY (id_asistencia) REFERENCES dbo.Asistencia (id_asistencia)
);
GO

-- Pedidos
CREATE TABLE dbo.Pedidos (
    id_pedido           INT           NOT NULL IDENTITY(1,1),
    id_mesa             INT               NULL,
    id_empleado         INT           NOT NULL,
    tipo_servicio       NVARCHAR(30)  NOT NULL CONSTRAINT DF_Ped_tipo   DEFAULT 'mesa',
    cantidad_comensales TINYINT           NULL,
    estado_pedido       NVARCHAR(30)  NOT NULL CONSTRAINT DF_Ped_estado DEFAULT 'abierto',
    fecha_hora          DATETIME2     NOT NULL CONSTRAINT DF_Ped_fecha  DEFAULT SYSUTCDATETIME(),
    observaciones       NVARCHAR(500)     NULL,
    estado              BIT           NOT NULL CONSTRAINT DF_Ped_activo DEFAULT 1,

    CONSTRAINT PK_Pedidos        PRIMARY KEY (id_pedido),
    CONSTRAINT FK_Ped_Mesa       FOREIGN KEY (id_mesa)     REFERENCES dbo.Mesas    (id_mesa),
    CONSTRAINT FK_Ped_Empleado   FOREIGN KEY (id_empleado) REFERENCES dbo.Empleados(id_empleado),
    CONSTRAINT CK_Ped_tipo       CHECK (tipo_servicio IN ('mesa','para_llevar','delivery')),
    CONSTRAINT CK_Ped_estado     CHECK (estado_pedido IN ('abierto','en_proceso','listo','entregado','cancelado'))
);
GO

-- Detalle_Pedido
CREATE TABLE dbo.Detalle_Pedido (
    id_detalle         INT            NOT NULL IDENTITY(1,1),
    id_pedido          INT            NOT NULL,
    id_producto        INT            NOT NULL,
    cantidad           DECIMAL(10,2)  NOT NULL CONSTRAINT CK_Det_cantidad CHECK (cantidad > 0),
    precio_unitario    DECIMAL(10,2)  NOT NULL CONSTRAINT CK_Det_precio   CHECK (precio_unitario >= 0),
    observaciones_item NVARCHAR(300)      NULL,
    estado_item        NVARCHAR(20)   NOT NULL CONSTRAINT DF_Det_estado_i DEFAULT 'pendiente',
    estado             BIT            NOT NULL CONSTRAINT DF_Det_estado   DEFAULT 1,

    CONSTRAINT PK_Detalle_Pedido  PRIMARY KEY (id_detalle),
    CONSTRAINT FK_Det_Pedido      FOREIGN KEY (id_pedido)   REFERENCES dbo.Pedidos  (id_pedido),
    CONSTRAINT FK_Det_Producto    FOREIGN KEY (id_producto) REFERENCES dbo.Productos(id_producto),
    CONSTRAINT CK_Det_estado_item CHECK (estado_item IN ('pendiente','preparando','listo','entregado','cancelado'))
);
GO

-- Subcuentas_Pedido
CREATE TABLE dbo.Subcuentas_Pedido (
    id_subcuenta     INT           NOT NULL IDENTITY(1,1),
    id_pedido        INT           NOT NULL,
    nombre_subcuenta NVARCHAR(50)  NOT NULL,
    estado           BIT           NOT NULL CONSTRAINT DF_SubCuentas_estado DEFAULT 1,

    CONSTRAINT PK_Subcuentas_Pedido PRIMARY KEY (id_subcuenta),
    CONSTRAINT FK_Sub_Pedido FOREIGN KEY (id_pedido) REFERENCES dbo.Pedidos (id_pedido)
);
GO

-- Cajas
CREATE TABLE dbo.Cajas (
    id_caja      INT          NOT NULL IDENTITY(1,1),
    nombre_caja  NVARCHAR(50) NOT NULL,
    estado_caja  NVARCHAR(20) NOT NULL CONSTRAINT DF_Cajas_estado_caja DEFAULT 'cerrada',
    estado       BIT          NOT NULL CONSTRAINT DF_Cajas_estado DEFAULT 1,

    CONSTRAINT PK_Cajas PRIMARY KEY (id_caja),
    CONSTRAINT UQ_Cajas_nombre UNIQUE (nombre_caja),
    CONSTRAINT CK_Cajas_estado CHECK (estado_caja IN ('abierta', 'cerrada', 'mantenimiento'))
);
GO

-- Apertura_Caja
CREATE TABLE dbo.Apertura_Caja (
    id_apertura    INT            NOT NULL IDENTITY(1,1),
    id_caja        INT            NOT NULL,
    id_cajero      INT            NOT NULL,
    monto_inicial  DECIMAL(12,2)  NOT NULL CONSTRAINT CK_Aper_monto CHECK (monto_inicial >= 0),
    fecha_apertura DATETIME2      NOT NULL CONSTRAINT DF_Aper_fecha  DEFAULT SYSUTCDATETIME(),
    observaciones  NVARCHAR(300)      NULL,
    estado         BIT            NOT NULL CONSTRAINT DF_Aper_estado DEFAULT 1,

    CONSTRAINT PK_Apertura_Caja PRIMARY KEY (id_apertura),
    CONSTRAINT FK_Aper_Caja     FOREIGN KEY (id_caja)   REFERENCES dbo.Cajas (id_caja),
    CONSTRAINT FK_Aper_Cajero   FOREIGN KEY (id_cajero) REFERENCES dbo.Empleados (id_empleado)
);
GO

-- Ventas
CREATE TABLE dbo.Ventas (
    id_venta       INT            NOT NULL IDENTITY(1,1),
    id_pedido      INT                NULL,
    id_subcuenta   INT                NULL,
    id_empleado    INT            NOT NULL,
    id_apertura    INT            NOT NULL,
    tipo_venta     NVARCHAR(30)   NOT NULL CONSTRAINT DF_Ven_tipo   DEFAULT 'normal',
    total_cobrado  DECIMAL(12,2)  NOT NULL CONSTRAINT CK_Ven_total  CHECK (total_cobrado  >= 0),
    monto_recibido DECIMAL(12,2)  NOT NULL CONSTRAINT DF_Ven_recib  DEFAULT 0,
    vuelto         DECIMAL(12,2)  NOT NULL CONSTRAINT DF_Ven_vuelto DEFAULT 0,
    metodo_pago    NVARCHAR(30)   NOT NULL,
    estado_venta   NVARCHAR(20)   NOT NULL CONSTRAINT DF_Ven_estado DEFAULT 'completada',
    fecha_hora     DATETIME2      NOT NULL CONSTRAINT DF_Ven_fecha  DEFAULT SYSUTCDATETIME(),
    estado         BIT            NOT NULL CONSTRAINT DF_Ven_activo DEFAULT 1,

    CONSTRAINT PK_Ventas        PRIMARY KEY (id_venta),
    CONSTRAINT FK_Ven_Pedido    FOREIGN KEY (id_pedido)    REFERENCES dbo.Pedidos      (id_pedido),
    CONSTRAINT FK_Ven_Subcuenta FOREIGN KEY (id_subcuenta) REFERENCES dbo.Subcuentas_Pedido (id_subcuenta),
    CONSTRAINT FK_Ven_Empleado  FOREIGN KEY (id_empleado)  REFERENCES dbo.Empleados    (id_empleado),
    CONSTRAINT FK_Ven_Apertura  FOREIGN KEY (id_apertura)  REFERENCES dbo.Apertura_Caja(id_apertura),
    CONSTRAINT CK_Ven_tipo      CHECK (tipo_venta   IN ('normal','cortesia','descuento')),
    CONSTRAINT CK_Ven_metodo    CHECK (metodo_pago  IN ('efectivo','tarjeta','sinpe','mixto')),
    CONSTRAINT CK_Ven_estado_v  CHECK (estado_venta IN ('completada','anulada','pendiente')),
    CONSTRAINT CK_Ven_vuelto    CHECK (vuelto >= 0)
);
GO

-- Notas_Credito
CREATE TABLE dbo.Notas_Credito (
    id_nota_credito INT            NOT NULL IDENTITY(1,1),
    id_venta        INT            NOT NULL,
    id_usuario      INT            NOT NULL,
    motivo          NVARCHAR(300)  NOT NULL,
    monto           DECIMAL(12,2)  NOT NULL CONSTRAINT CK_NC_monto CHECK (monto > 0),
    fecha_hora      DATETIME2      NOT NULL CONSTRAINT DF_NC_fecha  DEFAULT SYSUTCDATETIME(),
    estado          BIT            NOT NULL CONSTRAINT DF_NC_estado DEFAULT 1,

    CONSTRAINT PK_Notas_Credito PRIMARY KEY (id_nota_credito),
    CONSTRAINT FK_NC_Venta      FOREIGN KEY (id_venta)   REFERENCES dbo.Ventas   (id_venta),
    CONSTRAINT FK_NC_Usuario    FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios (id_usuario)
);
GO

-- Egresos_Caja
CREATE TABLE dbo.Egresos_Caja (
    id_egreso       INT            NOT NULL IDENTITY(1,1),
    id_apertura     INT            NOT NULL,
    id_usuario      INT            NOT NULL,
    categoria_gasto NVARCHAR(100)  NOT NULL,
    descripcion     NVARCHAR(300)      NULL,
    monto           DECIMAL(12,2)  NOT NULL CONSTRAINT CK_Egr_monto CHECK (monto > 0),
    fecha_hora      DATETIME2      NOT NULL CONSTRAINT DF_Egr_fecha  DEFAULT SYSUTCDATETIME(),
    estado          BIT            NOT NULL CONSTRAINT DF_Egr_estado DEFAULT 1,

    CONSTRAINT PK_Egresos_Caja PRIMARY KEY (id_egreso),
    CONSTRAINT FK_Egr_Apertura FOREIGN KEY (id_apertura) REFERENCES dbo.Apertura_Caja (id_apertura),
    CONSTRAINT FK_Egr_Usuario  FOREIGN KEY (id_usuario)  REFERENCES dbo.Usuarios      (id_usuario)
);
GO

-- Cierres_Caja
CREATE TABLE dbo.Cierres_Caja (
    id_cierre      INT            NOT NULL IDENTITY(1,1),
    id_cajero      INT            NOT NULL,
    id_apertura    INT            NOT NULL,
    fecha_cierre   DATETIME2      NOT NULL CONSTRAINT DF_Cier_fecha     DEFAULT SYSUTCDATETIME(),
    monto_apertura DECIMAL(12,2)  NOT NULL CONSTRAINT DF_Cier_apertura  DEFAULT 0,
    total_efectivo DECIMAL(12,2)  NOT NULL CONSTRAINT DF_Cier_efectivo  DEFAULT 0,
    total_sinpe    DECIMAL(12,2)  NOT NULL CONSTRAINT DF_Cier_sinpe     DEFAULT 0,
    total_tarjeta  DECIMAL(12,2)  NOT NULL CONSTRAINT DF_Cier_tarjeta   DEFAULT 0,
    total_egresos  DECIMAL(12,2)  NOT NULL CONSTRAINT DF_Cier_egresos   DEFAULT 0,
    saldo_esperado DECIMAL(12,2)  NOT NULL,
    saldo_real     DECIMAL(12,2)  NOT NULL,
    descuadre      BIT            NOT NULL CONSTRAINT DF_Cier_descuadre DEFAULT 0,
    estado         BIT            NOT NULL CONSTRAINT DF_Cier_estado    DEFAULT 1,

    CONSTRAINT PK_Cierres_Caja  PRIMARY KEY (id_cierre),
    CONSTRAINT FK_Cier_Cajero   FOREIGN KEY (id_cajero)   REFERENCES dbo.Empleados    (id_empleado),
    CONSTRAINT FK_Cier_Apertura FOREIGN KEY (id_apertura) REFERENCES dbo.Apertura_Caja(id_apertura),
    CONSTRAINT UQ_Cier_Apertura UNIQUE (id_apertura)
);
GO

-- Reportes_Generados
CREATE TABLE dbo.Reportes_Generados (
    id_reporte       INT            NOT NULL IDENTITY(1,1),
    id_usuario       INT            NOT NULL,
    tipo_reporte     NVARCHAR(50)   NOT NULL,
    parametros       NVARCHAR(500)      NULL,
    formato_salida   NVARCHAR(10)   NOT NULL CONSTRAINT DF_Rep_formato DEFAULT 'pdf',
    fecha_generacion DATETIME2      NOT NULL CONSTRAINT DF_Rep_fecha   DEFAULT SYSUTCDATETIME(),
    estado           BIT            NOT NULL CONSTRAINT DF_Rep_estado  DEFAULT 1,

    CONSTRAINT PK_Reportes    PRIMARY KEY (id_reporte),
    CONSTRAINT FK_Rep_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios (id_usuario),
    CONSTRAINT CK_Rep_tipo    CHECK (tipo_reporte IN ('ventas','productos_mas_vendidos','ingresos_metodo_pago',
                                                      'desempenio_meseros','inventario','egresos',
                                                      'cierre_turno','bitacora')),
    CONSTRAINT CK_Rep_formato CHECK (formato_salida IN ('pdf','excel','csv'))
);
GO

-- Bitacora_Inventario
CREATE TABLE dbo.Bitacora_Inventario (
    id_registro    INT            NOT NULL IDENTITY(1,1),
    id_usuario     INT            NOT NULL,
    accion         NVARCHAR(30)   NOT NULL,
    valor_anterior NVARCHAR(MAX)      NULL,
    valor_nuevo    NVARCHAR(MAX)      NULL,
    detalle        NVARCHAR(500)      NULL,
    ip_origen      NVARCHAR(50)       NULL,
    dispositivo    NVARCHAR(100)      NULL,
    fecha_hora     DATETIME2      NOT NULL CONSTRAINT DF_BitInv_fecha DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Bitacora_Inventario PRIMARY KEY (id_registro),
    CONSTRAINT FK_BitInv_Usuario      FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios (id_usuario),
    CONSTRAINT CK_BitInv_accion       CHECK (accion IN ('entrada','salida','ajuste','baja','activacion'))
);
GO

-- Bitacora_Financiera
CREATE TABLE dbo.Bitacora_Financiera (
    id_registro          INT            NOT NULL IDENTITY(1,1),
    id_usuario           INT            NOT NULL,
    tabla_afectada       NVARCHAR(80)   NOT NULL,
    id_registro_afectado INT            NOT NULL,
    accion               NVARCHAR(30)   NOT NULL,
    valor_anterior       NVARCHAR(MAX)      NULL,
    valor_nuevo          NVARCHAR(MAX)      NULL,
    detalle              NVARCHAR(500)      NULL,
    ip_origen            NVARCHAR(50)       NULL,
    dispositivo          NVARCHAR(100)      NULL,
    fecha_hora           DATETIME2      NOT NULL CONSTRAINT DF_BitFin_fecha DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Bitacora_Financiera PRIMARY KEY (id_registro),
    CONSTRAINT FK_BitFin_Usuario      FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios (id_usuario),
    CONSTRAINT CK_BitFin_accion       CHECK (accion IN ('INSERT','UPDATE','DELETE','ANULACION'))
);
GO

-- Bitacora_Usuarios_RRHH
CREATE TABLE dbo.Bitacora_Usuarios_RRHH (
    id_registro          INT            NOT NULL IDENTITY(1,1),
    id_usuario           INT            NOT NULL,
    tabla_afectada       NVARCHAR(80)   NOT NULL,
    id_registro_afectado INT            NOT NULL,
    accion               NVARCHAR(30)   NOT NULL,
    valor_anterior       NVARCHAR(MAX)      NULL,
    valor_nuevo          NVARCHAR(MAX)      NULL,
    detalle              NVARCHAR(500)      NULL,
    ip_origen            NVARCHAR(50)       NULL,
    dispositivo          NVARCHAR(100)      NULL,
    fecha_hora           DATETIME2      NOT NULL CONSTRAINT DF_BitRR_fecha DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Bitacora_RRHH PRIMARY KEY (id_registro),
    CONSTRAINT FK_BitRR_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios (id_usuario),
    CONSTRAINT CK_BitRR_accion  CHECK (accion IN ('INSERT','UPDATE','DELETE','ACTIVACION','DESACTIVACION'))
);
GO

-- Bitacora_Pedidos
CREATE TABLE dbo.Bitacora_Pedidos (
    id_registro     INT            NOT NULL IDENTITY(1,1),
    id_usuario      INT            NOT NULL,
    accion          NVARCHAR(30)   NOT NULL,
    valor_anterior  NVARCHAR(MAX)      NULL,
    valor_nuevo     NVARCHAR(MAX)      NULL,
    detalle         NVARCHAR(500)      NULL,
    ip_origen       NVARCHAR(50)       NULL,
    dispositivo     NVARCHAR(100)      NULL,
    fecha_hora      DATETIME2      NOT NULL CONSTRAINT DF_BitPed_fecha DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Bitacora_Pedidos PRIMARY KEY (id_registro),
    CONSTRAINT FK_BitPed_Usuario   FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios (id_usuario),
    CONSTRAINT CK_BitPed_accion    CHECK (accion IN ('CREACION','MODIFICACION','CANCELACION','ENTREGA','PAGO'))
);
GO

-- Bitacora_Acceso
CREATE TABLE dbo.Bitacora_Acceso (
    id_registro    INT            NOT NULL IDENTITY(1,1),
    id_usuario     INT            NOT NULL,
    accion         NVARCHAR(30)   NOT NULL,
    valor_anterior NVARCHAR(MAX)      NULL,
    valor_nuevo    NVARCHAR(MAX)      NULL,
    detalle        NVARCHAR(500)      NULL,
    ip_origen      NVARCHAR(50)       NULL,
    dispositivo    NVARCHAR(100)      NULL,
    fecha_hora     DATETIME2      NOT NULL CONSTRAINT DF_BitAcc_fecha     DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Bitacora_Acceso PRIMARY KEY (id_registro),
    CONSTRAINT FK_BitAcc_Usuario  FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios (id_usuario),
    CONSTRAINT CK_BitAcc_evento   CHECK (accion IN ('LOGIN','LOGOUT','BLOQUEO','SESION_EXPIRADA'))
);
GO

-- Bitacora_Reportes
CREATE TABLE dbo.Bitacora_Reportes (
    id_registro    INT            NOT NULL IDENTITY(1,1),
    id_usuario     INT            NOT NULL,
    accion         NVARCHAR(30)   NOT NULL,
    valor_anterior NVARCHAR(MAX)      NULL,
    valor_nuevo    NVARCHAR(MAX)      NULL,
    detalle        NVARCHAR(500)      NULL,
    ip_origen      NVARCHAR(50)       NULL,
    dispositivo    NVARCHAR(100)      NULL,
    fecha_hora     DATETIME2      NOT NULL CONSTRAINT DF_BitRep_fecha DEFAULT SYSUTCDATETIME(),

    CONSTRAINT PK_Bitacora_Reportes PRIMARY KEY (id_registro),
    CONSTRAINT FK_BitRep_Usuario    FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios (id_usuario),
    CONSTRAINT CK_BitRep_accion     CHECK (accion IN ('GENERACION','DESCARGA','VISUALIZACION'))
);
GO

CREATE INDEX IX_Pedidos_fecha        ON dbo.Pedidos               (fecha_hora   DESC);
CREATE INDEX IX_Ventas_fecha         ON dbo.Ventas                (fecha_hora   DESC);
CREATE INDEX IX_Ventas_apertura      ON dbo.Ventas                (id_apertura);
CREATE INDEX IX_Cierres_fecha        ON dbo.Cierres_Caja          (fecha_cierre DESC);
CREATE INDEX IX_Detalle_pedido       ON dbo.Detalle_Pedido        (id_pedido);
CREATE INDEX IX_Detalle_producto     ON dbo.Detalle_Pedido        (id_producto);
CREATE INDEX IX_Insumos_stock        ON dbo.Insumos               (stock_actual, stock_minimo);
CREATE INDEX IX_Productos_categoria  ON dbo.Productos             (id_categoria_prod);
CREATE INDEX IX_Productos_disponible ON dbo.Productos             (disponible) WHERE disponible = 1;
CREATE INDEX IX_Asistencia_empleado  ON dbo.Asistencia            (id_empleado, fecha_hora_entrada DESC);
CREATE INDEX IX_Turnos_fecha         ON dbo.Turnos_Trabajo        (fecha_turno  DESC);
CREATE INDEX IX_Vacaciones_estado    ON dbo.Vacaciones            (estado_solicitud, id_empleado);
CREATE INDEX IX_BitInv_fecha         ON dbo.Bitacora_Inventario   (fecha_hora   DESC);
CREATE INDEX IX_BitFin_fecha         ON dbo.Bitacora_Financiera   (fecha_hora   DESC);
CREATE INDEX IX_BitRR_fecha          ON dbo.Bitacora_Usuarios_RRHH(fecha_hora   DESC);
CREATE INDEX IX_BitPed_pedido        ON dbo.Bitacora_Pedidos      (fecha_hora DESC);
CREATE INDEX IX_BitAcc_usuario       ON dbo.Bitacora_Acceso       (id_usuario,  fecha_hora DESC);
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

CREATE OR ALTER VIEW dbo.v_stock_bajo AS
SELECT
    i.id_insumo, i.nombre_insumo, c.nombre_categoria,
    i.unidad_medida, i.stock_minimo, i.stock_actual,
    i.stock_minimo - i.stock_actual AS unidades_faltantes
FROM dbo.Insumos i
JOIN dbo.Categorias_Insumo c ON c.id_categoria = i.id_categoria
WHERE i.stock_actual < i.stock_minimo AND i.estado = 1;
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

-- Carga parametria inicial
INSERT INTO dbo.Roles (nombre_rol, descripcion) VALUES
    ('Administrador', 'Acceso total al sistema'),
    ('Cajero',        'Gestión de ventas y cierres de caja'),
    ('Mesero',        'Toma y gestión de pedidos en sala'),
    ('Cocinero',      'Visualización y actualización de pedidos en cocina'),
    ('Supervisor',    'Reportes y supervisión operativa');
GO

INSERT INTO dbo.Categorias_Insumo (nombre_categoria) VALUES
    ('Bebidas'), ('Carnes'), ('Lácteos'), ('Vegetales'),
    ('Granos y cereales'), ('Condimentos'), ('Postres'), ('Limpieza');
GO

INSERT INTO dbo.Categorias_Producto (nombre_categoria) VALUES
    ('Entradas'), ('Platos Fuertes'), ('Pastas'), ('Ensaladas'),
    ('Postres'), ('Bebidas frías'), ('Bebidas calientes'), ('Combos');
GO

-- Insertar Caja por defecto
INSERT INTO dbo.Cajas (nombre_caja, estado_caja) VALUES 
    ('Caja Principal Terminal 01', 'cerrada');
GO


