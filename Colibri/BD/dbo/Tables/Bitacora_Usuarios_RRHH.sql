CREATE TABLE [dbo].[Bitacora_Usuarios_RRHH] (
    [id_registro]          INT            IDENTITY (1, 1) NOT NULL,
    [id_usuario]           INT            NOT NULL,
    [tabla_afectada]       NVARCHAR (80)  NOT NULL,
    [id_registro_afectado] INT            NOT NULL,
    [accion]               NVARCHAR (30)  NOT NULL,
    [detalle]              NVARCHAR (500) NULL,
    [fecha_hora]           DATETIME2 (7)  CONSTRAINT [DF_BitRR_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Bitacora_RRHH] PRIMARY KEY CLUSTERED ([id_registro] ASC),
    CONSTRAINT [CK_BitRR_accion] CHECK ([accion]='DESACTIVACION' OR [accion]='ACTIVACION' OR [accion]='DELETE' OR [accion]='UPDATE' OR [accion]='INSERT'),
    CONSTRAINT [FK_BitRR_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);


GO
CREATE NONCLUSTERED INDEX [IX_BitRR_fecha]
    ON [dbo].[Bitacora_Usuarios_RRHH]([fecha_hora] DESC);

