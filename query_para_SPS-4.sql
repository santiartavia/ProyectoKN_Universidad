ALTER TABLE Pedidos DROP CONSTRAINT CK_Ped_estado;

ALTER TABLE Pedidos
ADD CONSTRAINT CK_Ped_estado
CHECK (
    estado_pedido IN ('cancelado', 'entregado', 'listo', 'en_proceso', 'abierto', 'finalizado')
);





ALTER TABLE Pedidos
ADD fecha_hora_finalizacion DATETIME2 NULL;


ALTER TABLE Ventas
ALTER COLUMN id_apertura INT NULL;





--- PARA REVISAR ID
SELECT id_empleado, id_usuario, nombre, apellidos
FROM Empleados;