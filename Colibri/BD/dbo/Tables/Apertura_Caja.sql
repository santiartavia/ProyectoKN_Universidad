CREATE TABLE [dbo].[Apertura_Caja] (
    [id_apertura]    INT             IDENTITY (1, 1) NOT NULL,
    [id_cajero]      INT             NOT NULL,
    [monto_inicial]  DECIMAL (12, 2) NOT NULL,
    [fecha_apertura] DATETIME2 (7)   CONSTRAINT [DF_Aper_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    [observaciones]  NVARCHAR (300)  NULL,
    [estado]         BIT             CONSTRAINT [DF_Aper_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Apertura_Caja] PRIMARY KEY CLUSTERED ([id_apertura] ASC),
    CONSTRAINT [CK_Aper_monto] CHECK ([monto_inicial]>=(0)),
    CONSTRAINT [FK_Aper_Cajero] FOREIGN KEY ([id_cajero]) REFERENCES [dbo].[Empleados] ([id_empleado])
);

