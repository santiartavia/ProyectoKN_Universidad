CREATE TABLE [dbo].[Asistencia] (
    [id_asistencia]      INT            IDENTITY (1, 1) NOT NULL,
    [id_empleado]        INT            NOT NULL,
    [id_turno]           INT            NULL,
    [fecha_hora_entrada] DATETIME2 (7)  CONSTRAINT [DF_Asist_entrada] DEFAULT (sysutcdatetime()) NOT NULL,
    [fecha_hora_salida]  DATETIME2 (7)  NULL,
    [observaciones]      NVARCHAR (300) NULL,
    [estado]             BIT            CONSTRAINT [DF_Asist_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Asistencia] PRIMARY KEY CLUSTERED ([id_asistencia] ASC),
    CONSTRAINT [FK_Asist_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado]),
    CONSTRAINT [FK_Asist_Turno] FOREIGN KEY ([id_turno]) REFERENCES [dbo].[Turnos_Trabajo] ([id_turno])
);


GO
CREATE NONCLUSTERED INDEX [IX_Asistencia_empleado]
    ON [dbo].[Asistencia]([id_empleado] ASC, [fecha_hora_entrada] DESC);

