CREATE TABLE [dbo].[Turnos_Trabajo] (
    [id_turno]    INT            IDENTITY (1, 1) NOT NULL,
    [id_empleado] INT            NOT NULL,
    [fecha_turno] DATE           NOT NULL,
    [hora_inicio] TIME (7)       NOT NULL,
    [hora_fin]    TIME (7)       NOT NULL,
    [descripcion] NVARCHAR (200) NULL,
    [estado]      BIT            CONSTRAINT [DF_Turnos_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Turnos_Trabajo] PRIMARY KEY CLUSTERED ([id_turno] ASC),
    CONSTRAINT [CK_Turnos_horas] CHECK ([hora_fin]>[hora_inicio]),
    CONSTRAINT [FK_Turnos_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado]),
    CONSTRAINT [UQ_Turnos_emp_dia] UNIQUE NONCLUSTERED ([id_empleado] ASC, [fecha_turno] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Turnos_fecha]
    ON [dbo].[Turnos_Trabajo]([fecha_turno] DESC);

