CREATE TABLE [dbo].[Bitacora_PDV] (
    [id_registro]      INT            IDENTITY (1, 1) NOT NULL,
    [id_caja]          INT            NOT NULL,
    [id_usuario_cajero] INT           NOT NULL,
    [accion_operativa] NVARCHAR (50)  NOT NULL,
    [id_venta]         INT            NULL,
    [detalle]          NVARCHAR (MAX) NULL,
    [fecha_hora]       DATETIME2 (7)  NOT NULL,
    [estado]           BIT            NOT NULL,
    CONSTRAINT [PK_Bitacora_PDV] PRIMARY KEY CLUSTERED ([id_registro] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_BitPDV_fecha]
    ON [dbo].[Bitacora_PDV]([fecha_hora] DESC);