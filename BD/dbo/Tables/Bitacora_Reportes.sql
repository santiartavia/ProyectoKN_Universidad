CREATE TABLE [dbo].[Bitacora_Reportes] (
    [id_registro] INT             IDENTITY (1, 1) NOT NULL,
    [id_usuario]  INT             NOT NULL,
    [accion]      NVARCHAR (30)   NOT NULL,
    [valor_anterior] NVARCHAR (MAX) NULL,
    [valor_nuevo] NVARCHAR (MAX)  NULL,
    [detalle]     NVARCHAR (500)  NULL,
    [ip_origen]   NVARCHAR (50)   NULL,
    [dispositivo] NVARCHAR (100)  NULL,
    [fecha_hora]  DATETIME2 (7)   CONSTRAINT [DF_BitRep_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Bitacora_Reportes] PRIMARY KEY CLUSTERED ([id_registro] ASC),
    CONSTRAINT [CK_BitRep_accion] CHECK ([accion]='VISUALIZACION' OR [accion]='DESCARGA' OR [accion]='GENERACION'),
    CONSTRAINT [FK_BitRep_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);
