CREATE TABLE [dbo].[Bitacora_Acceso] (
    [id_registro] INT            IDENTITY (1, 1) NOT NULL,
    [id_usuario]  INT            NOT NULL,
    [tipo_evento] NVARCHAR (20)  NOT NULL,
    [ip_origen]   NVARCHAR (50)  NULL,
    [dispositivo] NVARCHAR (100) NULL,
    [resultado]   NVARCHAR (20)  CONSTRAINT [DF_BitAcc_resultado] DEFAULT ('exitoso') NOT NULL,
    [fecha_hora]  DATETIME2 (7)  CONSTRAINT [DF_BitAcc_fecha] DEFAULT (sysutcdatetime()) NOT NULL,
    CONSTRAINT [PK_Bitacora_Acceso] PRIMARY KEY CLUSTERED ([id_registro] ASC),
    CONSTRAINT [CK_BitAcc_evento] CHECK ([tipo_evento]='SESION_EXPIRADA' OR [tipo_evento]='BLOQUEO' OR [tipo_evento]='LOGOUT' OR [tipo_evento]='LOGIN'),
    CONSTRAINT [CK_BitAcc_result] CHECK ([resultado]='fallido' OR [resultado]='exitoso'),
    CONSTRAINT [FK_BitAcc_Usuario] FOREIGN KEY ([id_usuario]) REFERENCES [dbo].[Usuarios] ([id_usuario])
);


GO
CREATE NONCLUSTERED INDEX [IX_BitAcc_usuario]
    ON [dbo].[Bitacora_Acceso]([id_usuario] ASC, [fecha_hora] DESC);

