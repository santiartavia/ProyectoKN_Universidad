CREATE TABLE [dbo].[Reportes_Generados] (
    [id_reporte]       INT            IDENTITY (1, 1) NOT NULL,
    [id_usuario]       INT            NOT NULL,
    [tipo_reporte]     NVARCHAR (50)  NOT NULL,
    [parametros]       NVARCHAR (500) NULL,
    [formato_salida]   NVARCHAR (10)  CONSTRAINT [DF_Rep_formato] DEFAULT ('pdf') NOT NULL,
    [fecha_generacion] DATETIME2 (7)  CONSTRAINT [DF_Rep_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    [estado]           BIT            CONSTRAINT [DF_Rep_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Reportes] PRIMARY KEY CLUSTERED ([id_reporte] ASC),
    CONSTRAINT [CK_Rep_formato] CHECK ([formato_salida]='csv' OR [formato_salida]='excel' OR [formato_salida]='pdf'),
    CONSTRAINT [CK_Rep_tipo] CHECK ([tipo_reporte]='bitacora' OR [tipo_reporte]='cierre_turno' OR [tipo_reporte]='egresos' OR [tipo_reporte]='inventario' OR [tipo_reporte]='desempenio_meseros' OR [tipo_reporte]='ingresos_metodo_pago' OR [tipo_reporte]='productos_mas_vendidos' OR [tipo_reporte]='ventas'),
    CONSTRAINT [FK_Rep_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);

