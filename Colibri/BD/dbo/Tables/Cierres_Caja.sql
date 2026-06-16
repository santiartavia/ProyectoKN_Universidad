CREATE TABLE [dbo].[Cierres_Caja] (
    [id_cierre]      INT             IDENTITY (1, 1) NOT NULL,
    [id_cajero]      INT             NOT NULL,
    [id_apertura]    INT             NOT NULL,
    [fecha_cierre]   DATETIME2 (7)   CONSTRAINT [DF_Cier_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    [monto_apertura] DECIMAL (12, 2) CONSTRAINT [DF_Cier_apertura] DEFAULT ((0)) NOT NULL,
    [total_efectivo] DECIMAL (12, 2) CONSTRAINT [DF_Cier_efectivo] DEFAULT ((0)) NOT NULL,
    [total_sinpe]    DECIMAL (12, 2) CONSTRAINT [DF_Cier_sinpe] DEFAULT ((0)) NOT NULL,
    [total_tarjeta]  DECIMAL (12, 2) CONSTRAINT [DF_Cier_tarjeta] DEFAULT ((0)) NOT NULL,
    [total_egresos]  DECIMAL (12, 2) CONSTRAINT [DF_Cier_egresos] DEFAULT ((0)) NOT NULL,
    [saldo_esperado] DECIMAL (12, 2) NOT NULL,
    [saldo_real]     DECIMAL (12, 2) NOT NULL,
    [descuadre]      BIT             CONSTRAINT [DF_Cier_descuadre] DEFAULT ((0)) NOT NULL,
    [estado]         BIT             CONSTRAINT [DF_Cier_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Cierres_Caja] PRIMARY KEY CLUSTERED ([id_cierre] ASC),
    CONSTRAINT [FK_Cier_Apertura] FOREIGN KEY ([id_apertura]) REFERENCES [dbo].[Apertura_Caja] ([id_apertura]),
    CONSTRAINT [FK_Cier_Cajero] FOREIGN KEY ([id_cajero]) REFERENCES [dbo].[Empleados] ([id_empleado]),
    CONSTRAINT [UQ_Cier_Apertura] UNIQUE NONCLUSTERED ([id_apertura] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Cierres_fecha]
    ON [dbo].[Cierres_Caja]([fecha_cierre] DESC);

