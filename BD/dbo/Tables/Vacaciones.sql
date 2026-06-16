CREATE TABLE [dbo].[Vacaciones] (
    [id_vacacion]      INT            IDENTITY (1, 1) NOT NULL,
    [id_empleado]      INT            NOT NULL,
    [id_aprobador]     INT            NULL,
    [fecha_inicio]     DATE           NOT NULL,
    [fecha_fin]        DATE           NOT NULL,
    [dias_solicitados] DECIMAL (5, 1) NOT NULL,
    [estado_solicitud] NVARCHAR (20)  CONSTRAINT [DF_Vac_estado] DEFAULT ('pendiente') NOT NULL,
    [motivo_rechazo]   NVARCHAR (300) NULL,
    [fecha_solicitud]  DATETIME2 (7)  CONSTRAINT [DF_Vac_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    [estado]           BIT            CONSTRAINT [DF_Vac_activo] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Vacaciones] PRIMARY KEY CLUSTERED ([id_vacacion] ASC),
    CONSTRAINT [CK_Vac_dias] CHECK ([dias_solicitados]>(0)),
    CONSTRAINT [CK_Vac_estado_sol] CHECK ([estado_solicitud]='rechazada' OR [estado_solicitud]='aprobada' OR [estado_solicitud]='pendiente'),
    CONSTRAINT [CK_Vac_fechas] CHECK ([fecha_fin]>=[fecha_inicio]),
    CONSTRAINT [FK_Vac_Aprobador] FOREIGN KEY ([id_aprobador]) REFERENCES [dbo].[Empleados] ([id_empleado]),
    CONSTRAINT [FK_Vac_Empleado] FOREIGN KEY ([id_empleado]) REFERENCES [dbo].[Empleados] ([id_empleado])
);


GO
CREATE NONCLUSTERED INDEX [IX_Vacaciones_estado]
    ON [dbo].[Vacaciones]([estado_solicitud] ASC, [id_empleado] ASC);

