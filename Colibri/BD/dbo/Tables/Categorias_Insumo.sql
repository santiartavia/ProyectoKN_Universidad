CREATE TABLE [dbo].[Categorias_Insumo] (
    [id_categoria]     INT            IDENTITY (1, 1) NOT NULL,
    [nombre_categoria] NVARCHAR (100) NOT NULL,
    [estado]           BIT            CONSTRAINT [DF_CatIns_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Categorias_Insumo] PRIMARY KEY CLUSTERED ([id_categoria] ASC),
    CONSTRAINT [UQ_CatIns_nombre] UNIQUE NONCLUSTERED ([nombre_categoria] ASC)
);

