CREATE TABLE [dbo].[Detalle_Pedido] (
    [id_detalle]         INT             IDENTITY (1, 1) NOT NULL,
    [id_pedido]          INT             NOT NULL,
    [id_producto]        INT             NOT NULL,
    [cantidad]           DECIMAL (10, 2) NOT NULL,
    [precio_unitario]    DECIMAL (10, 2) NOT NULL,
    [observaciones_item] NVARCHAR (300)  NULL,
    [estado_item]        NVARCHAR (20)   CONSTRAINT [DF_Det_estado_i] DEFAULT ('pendiente') NOT NULL,
    [estado]             BIT             CONSTRAINT [DF_Det_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Detalle_Pedido] PRIMARY KEY CLUSTERED ([id_detalle] ASC),
    CONSTRAINT [CK_Det_cantidad] CHECK ([cantidad]>(0)),
    CONSTRAINT [CK_Det_estado_item] CHECK ([estado_item]='cancelado' OR [estado_item]='entregado' OR [estado_item]='listo' OR [estado_item]='preparando' OR [estado_item]='pendiente'),
    CONSTRAINT [CK_Det_precio] CHECK ([precio_unitario]>=(0)),
    CONSTRAINT [FK_Det_Pedido] FOREIGN KEY ([id_pedido]) REFERENCES [dbo].[Pedidos] ([id_pedido]),
    CONSTRAINT [FK_Det_Producto] FOREIGN KEY ([id_producto]) REFERENCES [dbo].[Productos] ([id_producto])
);


GO
CREATE NONCLUSTERED INDEX [IX_Detalle_producto]
    ON [dbo].[Detalle_Pedido]([id_producto] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Detalle_pedido]
    ON [dbo].[Detalle_Pedido]([id_pedido] ASC);

