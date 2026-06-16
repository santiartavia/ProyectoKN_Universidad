CREATE TABLE [dbo].[Bitacora_Reportes] (
    [id_registro] INT           IDENTITY (1, 1) NOT NULL,
    [id_usuario]  INT           NOT NULL,
    [id_reporte]  INT           NOT NULL,
    [accion]      NVARCHAR (20) NOT NULL,
    [fecha_hora]  DATETIME2 (7) CONSTRAINT [DF_BitRep_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Bitacora_Reportes] PRIMARY KEY CLUSTERED ([id_registro] ASC),
    CONSTRAINT [CK_BitRep_accion] CHECK ([accion]='VISUALIZACION' OR [accion]='DESCARGA' OR [accion]='GENERACION'),
    CONSTRAINT [FK_BitRep_Reporte] FOREIGN KEY ([id_reporte]) REFERENCES [dbo].[Reportes_Generados] ([id_reporte]),
    CONSTRAINT [FK_BitRep_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);

