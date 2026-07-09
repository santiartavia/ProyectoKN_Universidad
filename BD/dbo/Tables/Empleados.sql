CREATE TABLE [dbo].[Empleados] (
    [id_empleado]                 INT             IDENTITY (1, 1) NOT NULL,
    [id_usuario]                  INT             NOT NULL,
    [cedula]                      NVARCHAR (20)   NOT NULL,
    [nombre]                      NVARCHAR (80)   NOT NULL,
    [apellidos]                   NVARCHAR (120)  NOT NULL,
    [telefono]                    NVARCHAR (20)   NULL,
    [correo_personal]             NVARCHAR (150)  NULL,
    [direccion]                   NVARCHAR (300)  NULL,
    [salario_hora]                DECIMAL (10, 2) NOT NULL,
    [dias_vacaciones_disponibles] DECIMAL (6, 2)  CONSTRAINT [DF_Emp_vac] DEFAULT ((0)) NOT NULL,
    [fecha_ingreso]               DATE            CONSTRAINT [DF_Emp_ingreso] DEFAULT (CONVERT([date],sysutcdatetime())) NOT NULL,
    [fecha_modificacion]          DATETIME2 (7)   NULL,
    [fecha_reactivacion]          DATETIME2 (7)   NULL,
    [motivo_inactivacion]         NVARCHAR (300)  NULL,
    [estado]                      BIT             CONSTRAINT [DF_Emp_estado] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_Empleados] PRIMARY KEY CLUSTERED ([id_empleado] ASC),
    CONSTRAINT [CK_Emp_salario] CHECK ([salario_hora]>=(0)),
    CONSTRAINT [FK_Emp_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario]),
    CONSTRAINT [UQ_Emp_cedula] UNIQUE NONCLUSTERED ([cedula] ASC),
    CONSTRAINT [UQ_Emp_usuario] UNIQUE NONCLUSTERED ([id_usuario] ASC)
);

