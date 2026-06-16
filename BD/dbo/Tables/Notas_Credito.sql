CREATE TABLE [dbo].[Notas_Credito] (
    [id_nota_credito] INT             IDENTITY (1, 1) NOT NULL,
    [id_venta]        INT             NOT NULL,
    [id_usuario]      INT             NOT NULL,
    [motivo]          NVARCHAR (300)  NOT NULL,
    [monto]           DECIMAL (12, 2) NOT NULL,
    [fecha_hora]      DATETIME2 (7)   CONSTRAINT [DF_NC_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    [estado]          BIT             CONSTRAINT [DF_NC_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Notas_Credito] PRIMARY KEY CLUSTERED ([id_nota_credito] ASC),
    CONSTRAINT [CK_NC_monto] CHECK ([monto]>(0)),
    CONSTRAINT [FK_NC_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario]),
    CONSTRAINT [FK_NC_Venta] FOREIGN KEY ([id_venta]) REFERENCES [dbo].[Ventas] ([id_venta])
);

