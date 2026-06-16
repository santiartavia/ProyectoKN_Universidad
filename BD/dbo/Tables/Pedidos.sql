CREATE TABLE [dbo].[Pedidos] (
    [id_pedido]           INT            IDENTITY (1, 1) NOT NULL,
    [id_mesa]             INT            NULL,
    [id_empleado]         INT            NOT NULL,
    [tipo_servicio]       NVARCHAR (30)  CONSTRAINT [DF_Ped_tipo] DEFAULT ('mesa') NOT NULL,
    [cantidad_comensales] TINYINT        NULL,
    [estado_pedido]       NVARCHAR (30)  CONSTRAINT [DF_Ped_estado] DEFAULT ('abierto') NOT NULL,
    [fecha_hora]          DATETIME2 (7)  CONSTRAINT [DF_Ped_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    [observaciones]       NVARCHAR (500) NULL,
    [estado]              BIT            CONSTRAINT [DF_Ped_activo] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Pedidos] PRIMARY KEY CLUSTERED ([id_pedido] ASC),
    CONSTRAINT [CK_Ped_estado] CHECK ([estado_pedido]='cancelado' OR [estado_pedido]='entregado' OR [estado_pedido]='listo' OR [estado_pedido]='en_proceso' OR [estado_pedido]='abierto'),
    CONSTRAINT [CK_Ped_tipo] CHECK ([tipo_servicio]='delivery' OR [tipo_servicio]='para_llevar' OR [tipo_servicio]='mesa'),
    CONSTRAINT [FK_Ped_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado]),
    CONSTRAINT [FK_Ped_Mesa] FOREIGN KEY ([id_mesa]) REFERENCES [dbo].[Mesas] ([id_mesa])
);


GO
CREATE NONCLUSTERED INDEX [IX_Pedidos_fecha]
    ON [dbo].[Pedidos]([fecha_hora] DESC);

