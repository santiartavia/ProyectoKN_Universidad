USE COLIBRI;
GO

ALTER TABLE dbo.Horas_Extra
ADD motivo_ajuste NVARCHAR(300) NULL;
GO

PRINT 'Columna motivo_ajuste agregada a Horas_Extra.';
GO
