CREATE TABLE [dbo].[Insumos] (
    [id_insumo]      INT             IDENTITY (1, 1) NOT NULL,
    [id_categoria]   INT             NOT NULL,
    [nombre_insumo]  NVARCHAR (120)  NOT NULL,
    [unidad_medida]  NVARCHAR (30)   NOT NULL,
    [stock_minimo]   DECIMAL (10, 2) CONSTRAINT [DF_Insumos_stock_min] DEFAULT ((0)) NOT NULL,
    [stock_actual]   DECIMAL (10, 2) CONSTRAINT [DF_Insumos_stock_act] DEFAULT ((0)) NOT NULL,
    [costo_unitario] DECIMAL (10, 2) CONSTRAINT [DF_Insumos_costo] DEFAULT ((0)) NOT NULL,
    [estado]         BIT             CONSTRAINT [DF_Insumos_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Insumos] PRIMARY KEY CLUSTERED ([id_insumo] ASC),
    CONSTRAINT [CK_Insumos_costo] CHECK ([costo_unitario]>=(0)),
    CONSTRAINT [CK_Insumos_stock_act] CHECK ([stock_actual]>=(0)),
    CONSTRAINT [CK_Insumos_stock_min] CHECK ([stock_minimo]>=(0)),
    CONSTRAINT [FK_Insumos_Categoria] FOREIGN KEY ([id_categoria]) REFERENCES [dbo].[Categorias_Insumo] ([id_categoria])
);


GO
CREATE NONCLUSTERED INDEX [IX_Insumos_stock]
    ON [dbo].[Insumos]([stock_actual] ASC, [stock_minimo] ASC);

