CREATE TABLE [dbo].[Roles] (
    [id_rol]      INT            IDENTITY (1, 1) NOT NULL,
    [nombre_rol]  NVARCHAR (60)  NOT NULL,
    [descripcion] NVARCHAR (255) NULL,
    [estado]      BIT            CONSTRAINT [DF_Roles_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY CLUSTERED ([id_rol] ASC),
    CONSTRAINT [UQ_Roles_nombre] UNIQUE NONCLUSTERED ([nombre_rol] ASC)
);

