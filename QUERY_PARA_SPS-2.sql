USE COLIBRI;

INSERT INTO Categorias_Producto (nombre_categoria, estado)
VALUES ('Menú Principal', 1);

DECLARE @idCategoria INT;
SET @idCategoria = SCOPE_IDENTITY();

INSERT INTO Productos (id_categoria_prod, nombre_producto, descripcion, precio_venta, disponible, estado)
VALUES
(@idCategoria, 'Carne con puré', 'Carne en salsa con puré de papa', 4500, 1, 1),
(@idCategoria, 'Pescado con vegetales', 'Filete de pescado con vegetales', 5200, 1, 1),
(@idCategoria, 'Casado de pollo', 'Casado tradicional con pollo', 3900, 1, 1),
(@idCategoria, 'Hamburguesa con papas', 'Hamburguesa con papas fritas', 4200, 1, 1),
(@idCategoria, 'Pasta Alfredo', 'Pasta con salsa Alfredo', 4800, 1, 1),
(@idCategoria, 'Arroz con camarones', 'Arroz salteado con camarones', 5500, 1, 1);



ALTER TABLE Pedidos DROP CONSTRAINT CK_Ped_tipo;

ALTER TABLE Pedidos
ADD CONSTRAINT CK_Ped_tipo
CHECK (
    tipo_servicio IN ('delivery', 'para_llevar', 'mesa', 'Salon', 'Para llevar')
);



INSERT INTO Mesas (numero_mesa, capacidad, estado_mesa, estado)
VALUES
('1', 4, 'disponible', 1),
('2', 4, 'disponible', 1),
('3', 4, 'disponible', 1),
('4', 4, 'disponible', 1),
('5', 4, 'disponible', 1),
('6', 4, 'disponible', 1),
('7', 4, 'disponible', 1),
('8', 4, 'disponible', 1),
('9', 4, 'disponible', 1),
('10', 4, 'disponible', 1),
('11', 4, 'disponible', 1),
('12', 4, 'disponible', 1);


SELECT id_mesa, numero_mesa, estado_mesa
FROM Mesas;