CREATE TABLE [dbo].[Egresos_Caja] (
    [id_egreso]       INT             IDENTITY (1, 1) NOT NULL,
    [id_apertura]     INT             NOT NULL,
    [id_usuario]      INT             NOT NULL,
    [categoria_gasto] NVARCHAR (100)  NOT NULL,
    [descripcion]     NVARCHAR (300)  NULL,
    [monto]           DECIMAL (12, 2) NOT NULL,
    [fecha_hora]      DATETIME2 (7)   CONSTRAINT [DF_Egr_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    [estado]          BIT             CONSTRAINT [DF_Egr_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Egresos_Caja] PRIMARY KEY CLUSTERED ([id_egreso] ASC),
    CONSTRAINT [CK_Egr_monto] CHECK ([monto]>(0)),
    CONSTRAINT [FK_Egr_Apertura] FOREIGN KEY ([id_apertura]) REFERENCES [dbo].[Apertura_Caja] ([id_apertura]),
    CONSTRAINT [FK_Egr_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);

