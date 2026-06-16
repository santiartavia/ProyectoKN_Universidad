CREATE TABLE [dbo].[Bitacora_Inventario] (
    [id_registro]    INT             IDENTITY (1, 1) NOT NULL,
    [id_usuario]     INT             NOT NULL,
    [id_insumo]      INT             NOT NULL,
    [accion]         NVARCHAR (30)   NOT NULL,
    [valor_anterior] DECIMAL (10, 2) NULL,
    [valor_nuevo]    DECIMAL (10, 2) NULL,
    [detalle]        NVARCHAR (300)  NULL,
    [fecha_hora]     DATETIME2 (7)   CONSTRAINT [DF_BitInv_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Bitacora_Inventario] PRIMARY KEY CLUSTERED ([id_registro] ASC),
    CONSTRAINT [CK_BitInv_accion] CHECK ([accion]='activacion' OR [accion]='baja' OR [accion]='ajuste' OR [accion]='salida' OR [accion]='entrada'),
    CONSTRAINT [FK_BitInv_Insumo] FOREIGN KEY ([id_insumo]) REFERENCES [dbo].[Insumos] ([id_insumo]),
    CONSTRAINT [FK_BitInv_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);


GO
CREATE NONCLUSTERED INDEX [IX_BitInv_fecha]
    ON [dbo].[Bitacora_Inventario]([fecha_hora] DESC);

