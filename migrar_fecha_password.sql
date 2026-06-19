USE COLIBRI;
GO

ALTER TABLE dbo.Usuarios
ADD fecha_password DATETIME2 NULL;
GO

PRINT 'Columna fecha_password agregada a Usuarios.';
GO
