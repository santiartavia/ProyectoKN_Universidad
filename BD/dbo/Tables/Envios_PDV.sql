CREATE TABLE [dbo].[Envios_PDV] (
    [id_envio]           INT            IDENTITY (1, 1) NOT NULL,
    [id_venta]           INT            NOT NULL,
    [id_usuario]         INT            NOT NULL,
    [destino]            NVARCHAR (20)  NOT NULL,
    [prioridad]          NVARCHAR (10)  NOT NULL,
    [estado_envio]       NVARCHAR (12)  NOT NULL,
    [codigo_confirmacion] NVARCHAR (50) NULL,
    [observaciones]      NVARCHAR (500) NULL,
    [motivo_reenvio]     NVARCHAR (300) NULL,
    [fecha_hora_envio]   DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_Envios_PDV] PRIMARY KEY CLUSTERED ([id_envio] ASC),
    CONSTRAINT [CK_EnvPDV_destino] CHECK ([destino] IN ('Caja', 'Bar', 'Cocina')),
    CONSTRAINT [CK_EnvPDV_prioridad] CHECK ([prioridad] IN ('Urgente', 'Normal')),
    CONSTRAINT [CK_EnvPDV_estado] CHECK ([estado_envio] IN ('Enviado', 'Fallido', 'Pendiente', 'Reenviado'))
);


GO
CREATE NONCLUSTERED INDEX [IX_EnvPDV_venta]
    ON [dbo].[Envios_PDV]([id_venta] ASC);