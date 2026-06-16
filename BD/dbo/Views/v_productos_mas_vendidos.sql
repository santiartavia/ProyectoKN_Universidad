
CREATE   VIEW dbo.v_productos_mas_vendidos AS
SELECT
    p.id_producto, p.nombre_producto, cp.nombre_categoria,
    SUM(dp.cantidad)                      AS total_unidades_vendidas,
    SUM(dp.cantidad * dp.precio_unitario) AS total_ingresos,
    COUNT(DISTINCT dp.id_pedido)          AS total_pedidos
FROM dbo.Detalle_Pedido dp
JOIN dbo.Productos          p  ON p.id_producto        = dp.id_producto
JOIN dbo.Categorias_Producto cp ON cp.id_categoria_prod = p.id_categoria_prod
WHERE dp.estado = 1 AND dp.estado_item <> 'cancelado'
GROUP BY p.id_producto, p.nombre_producto, cp.nombre_categoria;