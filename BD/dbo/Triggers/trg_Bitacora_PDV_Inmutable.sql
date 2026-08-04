CREATE TRIGGER [dbo].[trg_Bitacora_PDV_Inmutable]
    ON [dbo].[Bitacora_PDV]
    AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    RAISERROR('Los registros de la bitacora del punto de venta son inmutables: no se permite modificar ni eliminar.', 16, 1);
    ROLLBACK TRANSACTION;
END