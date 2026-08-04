CREATE TABLE [dbo].[Subcuenta_Detalle_Venta] (
    [id_subcuenta_detalle] INT            IDENTITY (1, 1) NOT NULL,
    [id_subcuenta]  INT            NOT NULL,
    [id_detalle_venta] INT           NOT NULL,
    [cantidad]      INT            NOT NULL,
    [subtotal]      DECIMAL (12, 2) NOT NULL,
    [fecha_operacion] DATETIME2 (7) NOT NULL,
    [estado]        BIT            NOT NULL,
    CONSTRAINT [PK_Subcuenta_Detalle_Venta] PRIMARY KEY CLUSTERED ([id_subcuenta_detalle] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_SCDV_subcuenta]
    ON [dbo].[Subcuenta_Detalle_Venta]([id_subcuenta] ASC);