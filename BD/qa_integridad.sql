-- Aplicar explícitamente sobre la base configurada, después de un respaldo.
-- No elimina datos ni crea empleados, cajas o saldos ficticios.
SET XACT_ABORT ON;
BEGIN TRANSACTION;
IF COL_LENGTH('dbo.Mesas', 'id_empleado_asignado') IS NULL
    ALTER TABLE dbo.Mesas ADD id_empleado_asignado INT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Mesas_MeseroAsignado')
    ALTER TABLE dbo.Mesas ADD CONSTRAINT FK_Mesas_MeseroAsignado
        FOREIGN KEY (id_empleado_asignado) REFERENCES dbo.Empleados(id_empleado);
IF OBJECT_ID('dbo.Solicitudes_Operacion', 'U') IS NULL
    CREATE TABLE dbo.Solicitudes_Operacion (
        IdSolicitud UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        IdUsuario INT NOT NULL REFERENCES dbo.Usuarios(id_usuario),
        Operacion NVARCHAR(150) NOT NULL,
        FechaHora DATETIME2 NOT NULL
    );
COMMIT;

-- Las bitácoras admiten inserciones, pero no cambios ni borrados desde la aplicación.
DECLARE @tabla SYSNAME, @sql NVARCHAR(MAX);
DECLARE auditorias CURSOR LOCAL FAST_FORWARD FOR
    SELECT name FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo') AND name IN
    ('Bitacora_Usuarios_RRHH', 'Bitacora_Pedidos', 'Bitacora_Inventario',
     'Bitacora_Acceso', 'Bitacora_PDV', 'Bitacora_Financiera', 'Bitacora_Reportes');
OPEN auditorias;
FETCH NEXT FROM auditorias INTO @tabla;
WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = N'CREATE OR ALTER TRIGGER dbo.' + QUOTENAME('TR_QA_Inmutable_' + @tabla)
        + N' ON dbo.' + QUOTENAME(@tabla) + N' INSTEAD OF UPDATE, DELETE AS BEGIN '
        + N'THROW 51010, ''Los registros de auditoría son inmutables.'', 1; END;';
    EXEC sys.sp_executesql @sql;
    FETCH NEXT FROM auditorias INTO @tabla;
END;
CLOSE auditorias;
DEALLOCATE auditorias;
