CREATE TABLE [dbo].[Categorias_Producto] (
    [id_categoria_prod] INT            IDENTITY (1, 1) NOT NULL,
    [nombre_categoria]  NVARCHAR (100) NOT NULL,
    [estado]            BIT            CONSTRAINT [DF_CatProd_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Categorias_Producto] PRIMARY KEY CLUSTERED ([id_categoria_prod] ASC),
    CONSTRAINT [UQ_CatProd_nombre] UNIQUE NONCLUSTERED ([nombre_categoria] ASC)
);

