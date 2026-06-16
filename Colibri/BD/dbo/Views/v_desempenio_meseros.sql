
CREATE   VIEW dbo.v_desempenio_meseros AS
SELECT
    e.id_empleado,
    e.nombre + ' ' + e.apellidos                                        AS nombre_completo,
    COUNT(DISTINCT ped.id_pedido)                                        AS total_pedidos,
    COUNT(DISTINCT ped.id_mesa)                                          AS mesas_atendidas,
    SUM(dp.cantidad * dp.precio_unitario)                                AS monto_total_vendido,
    AVG(CAST(DATEDIFF(MINUTE, ped.fecha_hora, v.fecha_hora) AS FLOAT))  AS tiempo_promedio_minutos
FROM dbo.Empleados e
JOIN dbo.Pedidos        ped ON ped.id_empleado = e.id_empleado AND ped.estado = 1
JOIN dbo.Detalle_Pedido dp  ON dp.id_pedido    = ped.id_pedido AND dp.estado  = 1
LEFT JOIN dbo.Ventas    v   ON v.id_pedido     = ped.id_pedido
GROUP BY e.id_empleado, e.nombre, e.apellidos;