CREATE TABLE [dbo].[Subcuentas_Venta] (
    [id_subcuenta]    INT            IDENTITY (1, 1) NOT NULL,
    [id_venta]        INT            NOT NULL,
    [nombre_subcuenta] NVARCHAR (100) NOT NULL,
    [subtotal]        DECIMAL (12, 2) NOT NULL,
    [pagada]          BIT            NOT NULL,
    [fecha_operacion] DATETIME2 (7)  NOT NULL,
    [estado]          BIT            NOT NULL,
    CONSTRAINT [PK_Subcuentas_Venta] PRIMARY KEY CLUSTERED ([id_subcuenta] ASC)
);


GO

GO
CREATE NONCLUSTERED INDEX [IX_SCV_venta]
    ON [dbo].[Subcuentas_Venta]([id_venta] ASC);