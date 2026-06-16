CREATE TABLE [dbo].[Bitacora_Financiera] (
    [id_registro]          INT            IDENTITY (1, 1) NOT NULL,
    [id_usuario]           INT            NOT NULL,
    [tabla_afectada]       NVARCHAR (80)  NOT NULL,
    [id_registro_afectado] INT            NOT NULL,
    [accion]               NVARCHAR (30)  NOT NULL,
    [detalle]              NVARCHAR (500) NULL,
    [fecha_hora]           DATETIME2 (7)  CONSTRAINT [DF_BitFin_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Bitacora_Financiera] PRIMARY KEY CLUSTERED ([id_registro] ASC),
    CONSTRAINT [CK_BitFin_accion] CHECK ([accion]='ANULACION' OR [accion]='DELETE' OR [accion]='UPDATE' OR [accion]='INSERT'),
    CONSTRAINT [FK_BitFin_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);


GO
CREATE NONCLUSTERED INDEX [IX_BitFin_fecha]
    ON [dbo].[Bitacora_Financiera]([fecha_hora] DESC);

