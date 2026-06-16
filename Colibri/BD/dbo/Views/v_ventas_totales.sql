
CREATE   VIEW dbo.v_ventas_totales AS
SELECT
    v.id_venta, v.id_pedido, v.id_empleado, v.id_apertura,
    v.tipo_venta, v.metodo_pago, v.estado_venta, v.fecha_hora,
    v.total_cobrado, v.monto_recibido, v.vuelto,
    SUM(dp.cantidad * dp.precio_unitario)        AS subtotal_calculado,
    SUM(dp.cantidad * dp.precio_unitario) * 0.13 AS iva_calculado,
    SUM(dp.cantidad * dp.precio_unitario) * 1.13 AS total_con_iva
FROM dbo.Ventas v
JOIN dbo.Detalle_Pedido dp ON dp.id_pedido = v.id_pedido AND dp.estado = 1
GROUP BY v.id_venta, v.id_pedido, v.id_empleado, v.id_apertura,
         v.tipo_venta, v.metodo_pago, v.estado_venta,
         v.fecha_hora, v.total_cobrado, v.monto_recibido, v.vuelto;