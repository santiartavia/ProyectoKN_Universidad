CREATE TABLE Historial_Estados_Pedido (
    id_historial INT IDENTITY(1,1) PRIMARY KEY,
    id_pedido INT NOT NULL,
    estado_anterior NVARCHAR(30) NULL,
    estado_nuevo NVARCHAR(30) NOT NULL,
    usuario_responsable NVARCHAR(100) NULL,
    fecha_hora_cambio DATETIME2 NOT NULL DEFAULT GETDATE(),
    detalle NVARCHAR(500) NULL,
    estado BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Historial_Estados_Pedido_Pedidos
        FOREIGN KEY (id_pedido) REFERENCES Pedidos(id_pedido)
);