CREATE TABLE [dbo].[Detalle_Venta] (
    [id_detalle_venta]  INT            IDENTITY (1, 1) NOT NULL,
    [id_venta]          INT            NOT NULL,
    [id_producto]       INT            NOT NULL,
    [cantidad]          INT            NOT NULL,
    [precio_unitario]   DECIMAL (12, 2) NOT NULL,
    [subtotal_item]     DECIMAL (12, 2) NOT NULL,
    [observaciones_item] NVARCHAR (MAX) NULL,
    [estado]            BIT            NOT NULL,
    CONSTRAINT [PK_Detalle_Venta] PRIMARY KEY CLUSTERED ([id_detalle_venta] ASC),
    CONSTRAINT [FK_DV_Venta] FOREIGN KEY ([id_venta]) REFERENCES [dbo].[Ventas] ([id_venta]),
    CONSTRAINT [FK_DV_Producto] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[Productos] ([id_producto])
);


GO
CREATE NONCLUSTERED INDEX [IX_DV_venta]
    ON [dbo].[Detalle_Venta]([id_venta] ASC);