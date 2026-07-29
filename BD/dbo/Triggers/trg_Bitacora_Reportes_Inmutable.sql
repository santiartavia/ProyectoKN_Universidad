CREATE TRIGGER [dbo].[trg_Bitacora_Reportes_Inmutable]
    ON [dbo].[Bitacora_Reportes]
    AFTER UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    RAISERROR('Los registros de la bitacora de reportes son inmutables: no se permite modificar ni eliminar.', 16, 1);
    ROLLBACK TRANSACTION;
END
