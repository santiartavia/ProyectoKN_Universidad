CREATE TABLE [dbo].[Ventas] (
    [id_venta]       INT             IDENTITY (1, 1) NOT NULL,
    [id_pedido]      INT             NOT NULL,
    [id_empleado]    INT             NOT NULL,
    [id_apertura]    INT             NOT NULL,
    [tipo_venta]     NVARCHAR (30)   CONSTRAINT [DF_Ven_tipo] DEFAULT ('normal') NOT NULL,
    [total_cobrado]  DECIMAL (12, 2) NOT NULL,
    [monto_recibido] DECIMAL (12, 2) CONSTRAINT [DF_Ven_recib] DEFAULT ((0)) NOT NULL,
    [vuelto]         DECIMAL (12, 2) CONSTRAINT [DF_Ven_vuelto] DEFAULT ((0)) NOT NULL,
    [metodo_pago]    NVARCHAR (30)   NOT NULL,
    [estado_venta]   NVARCHAR (20)   CONSTRAINT [DF_Ven_estado] DEFAULT ('completada') NOT NULL,
    [fecha_hora]     DATETIME2 (7)   CONSTRAINT [DF_Ven_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    [estado]         BIT             CONSTRAINT [DF_Ven_activo] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Ventas] PRIMARY KEY CLUSTERED ([id_venta] ASC),
    CONSTRAINT [CK_Ven_estado_v] CHECK ([estado_venta]='pendiente' OR [estado_venta]='anulada' OR [estado_venta]='completada'),
    CONSTRAINT [CK_Ven_metodo] CHECK ([metodo_pago]='mixto' OR [metodo_pago]='sinpe' OR [metodo_pago]='tarjeta' OR [metodo_pago]='efectivo'),
    CONSTRAINT [CK_Ven_tipo] CHECK ([tipo_venta]='descuento' OR [tipo_venta]='cortesia' OR [tipo_venta]='normal'),
    CONSTRAINT [CK_Ven_total] CHECK ([total_cobrado]>=(0)),
    CONSTRAINT [CK_Ven_vuelto] CHECK ([vuelto]>=(0)),
    CONSTRAINT [FK_Ven_Apertura] FOREIGN KEY ([id_apertura]) REFERENCES [dbo].[Apertura_Caja] ([id_apertura]),
    CONSTRAINT [FK_Ven_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado]),
    CONSTRAINT [FK_Ven_Pedido] FOREIGN KEY ([id_pedido]) REFERENCES [dbo].[Pedidos] ([id_pedido]),
    CONSTRAINT [UQ_Ventas_pedido] UNIQUE NONCLUSTERED ([id_pedido] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Ventas_apertura]
    ON [dbo].[Ventas]([id_apertura] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Ventas_fecha]
    ON [dbo].[Ventas]([fecha_hora] DESC);

