USE COLIBRI;
GO
-- Agregar columna id_pedido faltante
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bitacora_Pedidos') AND name = 'id_pedido')
    ALTER TABLE dbo.Bitacora_Pedidos ADD id_pedido INT NOT NULL CONSTRAINT DF_BP_id_pedido DEFAULT 0;
GO
-- Renombrar valor_anterior -> estado_anterior
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bitacora_Pedidos') AND name = 'valor_anterior')
    EXEC sp_rename 'dbo.Bitacora_Pedidos.valor_anterior', 'estado_anterior', 'COLUMN';
GO
-- Renombrar valor_nuevo -> estado_nuevo
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Bitacora_Pedidos') AND name = 'valor_nuevo')
    EXEC sp_rename 'dbo.Bitacora_Pedidos.valor_nuevo', 'estado_nuevo', 'COLUMN';
GO
-- ALTER TABLE dbo.Bitacora_Pedidos DROP COLUMN ip_origen, dispositivo;
GO
-- Eliminar default temporal
ALTER TABLE dbo.Bitacora_Pedidos DROP CONSTRAINT DF_BP_id_pedido;
GO
