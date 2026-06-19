USE COLIBRI;
GO

ALTER TABLE dbo.Horas_Extra DROP COLUMN monto_calculado;
GO

ALTER TABLE dbo.Horas_Extra ADD monto_calculado DECIMAL(12,2) NULL;
GO

PRINT 'Columna monto_calculado convertida de computed a regular.';
GO
