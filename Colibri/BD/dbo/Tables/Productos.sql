CREATE TABLE [dbo].[Productos] (
    [id_producto]       INT             IDENTITY (1, 1) NOT NULL,
    [id_categoria_prod] INT             NOT NULL,
    [nombre_producto]   NVARCHAR (120)  NOT NULL,
    [descripcion]       NVARCHAR (500)  NULL,
    [precio_venta]      DECIMAL (10, 2) NOT NULL,
    [disponible]        BIT             CONSTRAINT [DF_Prod_disp] DEFAULT ((1)) NOT NULL,
    [estado]            BIT             CONSTRAINT [DF_Prod_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Productos] PRIMARY KEY CLUSTERED ([id_producto] ASC),
    CONSTRAINT [CK_Prod_precio] CHECK ([precio_venta]>=(0)),
    CONSTRAINT [FK_Prod_Categoria] FOREIGN KEY ([id_categoria_prod]) REFERENCES [dbo].[Categorias_Producto] ([id_categoria_prod])
);


GO
CREATE NONCLUSTERED INDEX [IX_Productos_disponible]
    ON [dbo].[Productos]([disponible] ASC) WHERE ([disponible]=(1));


GO
CREATE NONCLUSTERED INDEX [IX_Productos_categoria]
    ON [dbo].[Productos]([id_categoria_prod] ASC);

