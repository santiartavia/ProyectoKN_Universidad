USE COLIBRI;
GO

IF OBJECT_ID('dbo.Cierres_Periodo', 'U') IS NOT NULL DROP TABLE dbo.Cierres_Periodo;
GO

CREATE TABLE dbo.Cierres_Periodo (
    id_cierre_periodo INT            IDENTITY(1,1) NOT NULL,
    id_usuario        INT            NOT NULL,
    tipo_periodo      NVARCHAR(10)   NOT NULL,
    mes               INT            NULL,
    anio              INT            NOT NULL,
    fecha_cierre      DATETIME2      NOT NULL CONSTRAINT DF_CPer_fecha DEFAULT SYSUTCDATETIME(),
    total_ingresos    DECIMAL(12,2)  NOT NULL CONSTRAINT DF_CPer_ingresos DEFAULT 0,
    total_egresos     DECIMAL(12,2)  NOT NULL CONSTRAINT DF_CPer_egresos DEFAULT 0,
    total_notas_credito DECIMAL(12,2) NOT NULL CONSTRAINT DF_CPer_nc DEFAULT 0,
    saldo_final       DECIMAL(12,2)  NOT NULL CONSTRAINT DF_CPer_saldo DEFAULT 0,
    estado            BIT            NOT NULL CONSTRAINT DF_CPer_estado DEFAULT 1,

    CONSTRAINT PK_Cierres_Periodo PRIMARY KEY (id_cierre_periodo),
    CONSTRAINT FK_CPer_Usuario      FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios (id_usuario),
    CONSTRAINT CK_CPer_tipo         CHECK (tipo_periodo IN ('mensual', 'anual')),
    CONSTRAINT CK_CPer_mes          CHECK (mes IS NULL OR (mes >= 1 AND mes <= 12)),
    CONSTRAINT UQ_CPer_periodo      UNIQUE (tipo_periodo, mes, anio)
);
GO
