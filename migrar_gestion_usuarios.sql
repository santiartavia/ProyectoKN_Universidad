USE COLIBRI;
GO
IF OBJECT_ID('dbo.PasswordHistorial', 'U') IS NOT NULL DROP TABLE dbo.PasswordHistorial;
CREATE TABLE dbo.PasswordHistorial (
    id_historial      INT            NOT NULL IDENTITY(1,1),
    id_usuario        INT            NOT NULL,
    password_hash     NVARCHAR(255)  NOT NULL,
    fecha_cambio      DATETIME2      NOT NULL CONSTRAINT DF_PH_fecha DEFAULT SYSUTCDATETIME(),
    metodo_cambio     NVARCHAR(20)   NOT NULL CONSTRAINT DF_PH_metodo DEFAULT 'MANUAL',
    dispositivo       NVARCHAR(100)  NULL,
    direccion_ip      NVARCHAR(50)   NULL,
    CONSTRAINT PK_PasswordHistorial PRIMARY KEY (id_historial),
    CONSTRAINT FK_PH_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios(id_usuario),
    CONSTRAINT CK_PH_metodo CHECK (metodo_cambio IN ('MANUAL','OBLIGATORIO','PRIMER_INGRESO'))
);
GO
IF OBJECT_ID('dbo.Sesiones', 'U') IS NOT NULL DROP TABLE dbo.Sesiones;
CREATE TABLE dbo.Sesiones (
    id_sesion                INT           NOT NULL IDENTITY(1,1),
    id_usuario               INT           NOT NULL,
    estado_sesion            NVARCHAR(20)  NOT NULL CONSTRAINT DF_Ses_estado DEFAULT 'ACTIVA',
    fecha_hora_inicio        DATETIME2     NOT NULL CONSTRAINT DF_Ses_inicio DEFAULT SYSUTCDATETIME(),
    fecha_hora_ultima_act    DATETIME2     NOT NULL CONSTRAINT DF_Ses_ult_act DEFAULT SYSUTCDATETIME(),
    fecha_hora_cierre        DATETIME2     NULL,
    dispositivo_acceso       NVARCHAR(100) NULL,
    direccion_ip             NVARCHAR(50)  NULL,
    motivo_cierre            NVARCHAR(50)  NULL,
    CONSTRAINT PK_Sesiones PRIMARY KEY (id_sesion),
    CONSTRAINT FK_Ses_Usuario FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios(id_usuario),
    CONSTRAINT CK_Ses_estado CHECK (estado_sesion IN ('ACTIVA','INACTIVA','CERRADA','EXPIRADA'))
);
GO
ALTER TABLE dbo.Bitacora_Usuarios_RRHH DROP CONSTRAINT CK_BitRR_accion;
ALTER TABLE dbo.Bitacora_Usuarios_RRHH ADD CONSTRAINT CK_BitRR_accion
    CHECK (accion IN ('INSERT','UPDATE','DELETE','ACTIVACION','DESACTIVACION','PASSWORD_CHANGE','ROLE_ASSIGNMENT'));
GO
-- Migrar Bitacora_Acceso
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'C' AND name = 'CK_BitAcc_evento')
    ALTER TABLE dbo.Bitacora_Acceso DROP CONSTRAINT CK_BitAcc_evento;
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'C' AND name = 'CK_BitAcc_result')
    ALTER TABLE dbo.Bitacora_Acceso DROP CONSTRAINT CK_BitAcc_result;
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bitacora_Acceso') AND name = 'accion')
    ALTER TABLE dbo.Bitacora_Acceso ADD accion NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bitacora_Acceso') AND name = 'valor_anterior')
    ALTER TABLE dbo.Bitacora_Acceso ADD valor_anterior NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bitacora_Acceso') AND name = 'valor_nuevo')
    ALTER TABLE dbo.Bitacora_Acceso ADD valor_nuevo NVARCHAR(MAX) NULL;
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bitacora_Acceso') AND name = 'detalle')
    ALTER TABLE dbo.Bitacora_Acceso ADD detalle NVARCHAR(MAX) NULL;
GO
-- Migrar datos desde tipo_evento a accion si la columna vieja existe
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bitacora_Acceso') AND name = 'tipo_evento')
BEGIN
    UPDATE dbo.Bitacora_Acceso SET accion = tipo_evento WHERE accion IS NULL;
    ALTER TABLE dbo.Bitacora_Acceso DROP COLUMN tipo_evento;
END
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bitacora_Acceso') AND name = 'resultado')
    ALTER TABLE dbo.Bitacora_Acceso DROP COLUMN resultado;
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bitacora_Acceso') AND name = 'accion')
BEGIN
    ALTER TABLE dbo.Bitacora_Acceso ALTER COLUMN accion NVARCHAR(50) NOT NULL;
    ALTER TABLE dbo.Bitacora_Acceso ADD CONSTRAINT CK_BitAcc_evento
        CHECK (accion IN ('LOGIN','LOGOUT','BLOQUEO','SESION_EXPIRADA','INTENTO_FALLIDO','FORGOT_PASSWORD','PASSWORD_RESET','LOGIN_SUCCESS'));
END
GO
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'fecha_aviso_password')
BEGIN
    ALTER TABLE dbo.Usuarios ADD fecha_aviso_password DATETIME2 NULL;
END
GO
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuarios') AND name = 'ultimo_cambio_password_ip')
BEGIN
    ALTER TABLE dbo.Usuarios ADD ultimo_cambio_password_ip NVARCHAR(50) NULL;
    ALTER TABLE dbo.Usuarios ADD ultimo_cambio_password_dispositivo NVARCHAR(100) NULL;
END
GO
CREATE INDEX IX_PH_usuario_fecha ON dbo.PasswordHistorial (id_usuario, fecha_cambio DESC);
CREATE INDEX IX_Ses_usuario_estado ON dbo.Sesiones (id_usuario, estado_sesion);
CREATE INDEX IX_Ses_fecha_ult_act ON dbo.Sesiones (fecha_hora_ultima_act);
GO

