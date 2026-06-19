USE COLIBRI;
GO

ALTER TABLE dbo.Empleados
ADD fecha_modificacion DATETIME2 NULL,
    fecha_reactivacion DATETIME2 NULL,
    motivo_inactivacion NVARCHAR(300) NULL;
GO

PRINT 'Columnas agregadas correctamente a Empleados.';
GO
