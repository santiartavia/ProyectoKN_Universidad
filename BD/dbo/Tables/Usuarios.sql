CREATE TABLE [dbo].[Usuarios] (
    [id_usuario]                INT            IDENTITY (1, 1) NOT NULL,
    [id_rol]                    INT            NOT NULL,
    [nombre_usuario]            NVARCHAR (80)  NOT NULL,
    [correo]                    NVARCHAR (150) NOT NULL,
    [password_hash]             NVARCHAR (255) NOT NULL,
    [cambio_password_requerido] BIT            CONSTRAINT [DF_Usr_cambio_pwd] DEFAULT ((0)) NOT NULL,
    [intentos_fallidos]         TINYINT        CONSTRAINT [DF_Usr_intentos] DEFAULT ((0)) NOT NULL,
    [bloqueado]                 BIT            CONSTRAINT [DF_Usr_bloqueado] DEFAULT ((0)) NOT NULL,
    [fecha_ultimo_acceso]       DATETIME2 (7)  NULL,
    [fecha_password]            DATETIME2 (7)  NULL,
    [estado]                    BIT            CONSTRAINT [DF_Usuarios_estado] DEFAULT ((1)) NOT NULL,
    [fecha_creacion]            DATETIME2 (7)  CONSTRAINT [DF_Usuarios_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    [direccion]                 NVARCHAR (300) NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY CLUSTERED ([id_usuario] ASC),
    CONSTRAINT [FK_Usuarios_Rol] FOREIGN KEY ([id_rol]) REFERENCES [dbo].[Roles] ([id_rol]),
    CONSTRAINT [UQ_Usuarios_correo] UNIQUE NONCLUSTERED ([correo] ASC),
    CONSTRAINT [UQ_Usuarios_nombre] UNIQUE NONCLUSTERED ([nombre_usuario] ASC)
);

