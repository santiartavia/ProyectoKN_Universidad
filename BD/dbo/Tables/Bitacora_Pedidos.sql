CREATE TABLE [dbo].[Bitacora_Pedidos] (
    [id_registro]     INT            IDENTITY (1, 1) NOT NULL,
    [id_usuario]      INT            NOT NULL,
    [id_pedido]       INT            NOT NULL,
    [accion]          NVARCHAR (30)  NOT NULL,
    [estado_anterior] NVARCHAR (30)  NULL,
    [estado_nuevo]    NVARCHAR (30)  NULL,
    [detalle]         NVARCHAR (500) NULL,
    [fecha_hora]      DATETIME2 (7)  CONSTRAINT [DF_BitPed_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Bitacora_Pedidos] PRIMARY KEY CLUSTERED ([id_registro] ASC),
    CONSTRAINT [CK_BitPed_accion] CHECK ([accion]='PAGO' OR [accion]='ENTREGA' OR [accion]='CANCELACION' OR [accion]='MODIFICACION' OR [accion]='CREACION'),
    CONSTRAINT [FK_BitPed_Pedido] FOREIGN KEY ([id_pedido]) REFERENCES [dbo].[Pedidos] ([id_pedido]),
    CONSTRAINT [FK_BitPed_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);


GO
CREATE NONCLUSTERED INDEX [IX_BitPed_pedido]
    ON [dbo].[Bitacora_Pedidos]([id_pedido] ASC, [fecha_hora] DESC);

