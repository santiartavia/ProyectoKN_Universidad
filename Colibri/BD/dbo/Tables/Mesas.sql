CREATE TABLE [dbo].[Mesas] (
    [id_mesa]     INT           IDENTITY (1, 1) NOT NULL,
    [numero_mesa] NVARCHAR (10) NOT NULL,
    [capacidad]   TINYINT       NOT NULL,
    [estado_mesa] NVARCHAR (20) CONSTRAINT [DF_Mesas_estado_mesa] DEFAULT ('disponible') NOT NULL,
    [estado]      BIT           CONSTRAINT [DF_Mesas_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Mesas] PRIMARY KEY CLUSTERED ([id_mesa] ASC),
    CONSTRAINT [CK_Mesas_capacidad] CHECK ([capacidad]>(0)),
    CONSTRAINT [CK_Mesas_estado_mesa] CHECK ([estado_mesa]='inactiva' OR [estado_mesa]='sucia' OR [estado_mesa]='reservada' OR [estado_mesa]='ocupada' OR [estado_mesa]='disponible'),
    CONSTRAINT [UQ_Mesas_numero] UNIQUE NONCLUSTERED ([numero_mesa] ASC)
);

