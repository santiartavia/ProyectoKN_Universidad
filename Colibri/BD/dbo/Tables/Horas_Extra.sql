CREATE TABLE [dbo].[Horas_Extra] (
    [id_hora_extra]   INT            IDENTITY (1, 1) NOT NULL,
    [id_asistencia]   INT            NOT NULL,
    [id_empleado]     INT            NOT NULL,
    [cantidad_horas]  DECIMAL (5, 2) NOT NULL,
    [factor_pago]     DECIMAL (4, 2) CONSTRAINT [DF_HE_factor] DEFAULT ((1.50)) NOT NULL,
    [monto_calculado] AS             ([cantidad_horas]*[factor_pago]),
    [fecha_registro]  DATETIME2 (7)  CONSTRAINT [DF_HE_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    [estado]          BIT            CONSTRAINT [DF_HE_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Horas_Extra] PRIMARY KEY CLUSTERED ([id_hora_extra] ASC),
    CONSTRAINT [CK_HE_horas] CHECK ([cantidad_horas]>(0)),
    CONSTRAINT [FK_HE_Asistencia] FOREIGN KEY ([id_asistencia]) REFERENCES [dbo].[Asistencia] ([id_asistencia]),
    CONSTRAINT [FK_HE_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado])
);

