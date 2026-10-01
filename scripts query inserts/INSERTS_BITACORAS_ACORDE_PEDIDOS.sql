-- =====================================================================================
-- BITACORAS REALISTAS ACORDES A PEDIDOS, VENTAS E INVENTARIO
-- SOLO INSERTS. NO ELIMINA NI MODIFICA REGISTROS EXISTENTES.
-- =====================================================================================

USE [COLIBRI];
GO

SET NOCOUNT ON;
GO

-- =====================================================================================
-- 1) BITACORA_PEDIDOS
--    Inserta UN registro base por cada pedido que todavía no tenga una CREACION.
--    Usa el usuario del mesero y exactamente la fecha del pedido.
-- =====================================================================================
INSERT INTO dbo.Bitacora_Pedidos
(
    id_usuario,
    accion,
    estado_anterior,
    estado_nuevo,
    detalle,
    ip_origen,
    dispositivo,
    fecha_hora,
    id_pedido
)
SELECT
    u.id_usuario,
    N'CREACION',
    NULL,
    N'abierto',
    CASE p.tipo_servicio
        WHEN N'mesa' THEN
            CONCAT(
                N'Pedido registrado para mesa ',
                ISNULL(m.numero_mesa, N'sin asignar'),
                N'. Cantidad de comensales: ',
                ISNULL(CAST(p.cantidad_comensales AS NVARCHAR(10)), N'no indicada'),
                N'.'
            )
        WHEN N'para_llevar' THEN
            N'Pedido para llevar registrado correctamente.'
        WHEN N'delivery' THEN
            N'Pedido para entrega a domicilio registrado correctamente.'
        ELSE
            N'Pedido registrado correctamente.'
    END,
    CONCAT(N'192.168.20.', 30 + (p.id_pedido % 20)),
    CASE p.id_pedido % 3
        WHEN 0 THEN N'Tablet de salón'
        WHEN 1 THEN N'Terminal de pedidos'
        ELSE N'Aplicación web'
    END,
    p.fecha_hora,
    p.id_pedido
FROM dbo.Pedidos p
INNER JOIN dbo.Empleados e
    ON e.id_empleado = p.id_empleado
INNER JOIN dbo.Usuarios u
    ON u.id_usuario = e.id_usuario
LEFT JOIN dbo.Mesas m
    ON m.id_mesa = p.id_mesa
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Bitacora_Pedidos bp
    WHERE bp.id_pedido = p.id_pedido
      AND bp.accion = N'CREACION'
);
GO


-- =====================================================================================
-- 2) BITACORA_PEDIDOS - ESTADO FINAL
--    Añade otro movimiento para pedidos finalizados, entregados o cancelados.
--    Esto hace que una parte de los pedidos tenga más de una entrada de auditoría,
--    como ocurriría en una operación real.
-- =====================================================================================
INSERT INTO dbo.Bitacora_Pedidos
(
    id_usuario,
    accion,
    estado_anterior,
    estado_nuevo,
    detalle,
    ip_origen,
    dispositivo,
    fecha_hora,
    id_pedido
)
SELECT
    u.id_usuario,

    CASE
        WHEN p.estado_pedido = N'cancelado' THEN N'CANCELACION'
        WHEN p.estado_pedido IN (N'finalizado', N'entregado') THEN N'ENTREGA'
        ELSE N'MODIFICACION'
    END,

    CASE
        WHEN p.estado_pedido = N'cancelado' THEN N'abierto'
        WHEN p.estado_pedido IN (N'finalizado', N'entregado') THEN N'listo'
        ELSE N'abierto'
    END,

    p.estado_pedido,

    CASE
        WHEN p.estado_pedido = N'cancelado'
            THEN N'Pedido cancelado durante la operación.'
        WHEN p.estado_pedido IN (N'finalizado', N'entregado')
            THEN N'Pedido entregado al cliente y finalizado correctamente.'
        ELSE
            N'Estado del pedido actualizado durante la operación.'
    END,

    CONCAT(N'192.168.20.', 30 + (p.id_pedido % 20)),
    N'Terminal de pedidos',

    CASE
        WHEN p.fecha_hora_finalizacion IS NOT NULL THEN p.fecha_hora_finalizacion
        WHEN p.fecha_hora_entrega IS NOT NULL THEN p.fecha_hora_entrega
        ELSE DATEADD(MINUTE, 30 + (p.id_pedido % 20), p.fecha_hora)
    END,

    p.id_pedido
FROM dbo.Pedidos p
INNER JOIN dbo.Empleados e
    ON e.id_empleado = p.id_empleado
INNER JOIN dbo.Usuarios u
    ON u.id_usuario = e.id_usuario
WHERE p.estado_pedido IN
(
    N'finalizado',
    N'entregado',
    N'cancelado',
    N'listo',
    N'en_proceso'
)
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.Bitacora_Pedidos bp
    WHERE bp.id_pedido = p.id_pedido
      AND bp.estado_nuevo = p.estado_pedido
);
GO


-- =====================================================================================
-- 3) BITACORA_PDV
--    Un movimiento de COBRO por cada venta.
--    Por ejemplo: si tienes 100 ventas, esta bitácora puede quedar cerca de 100 filas.
-- =====================================================================================
INSERT INTO dbo.Bitacora_PDV
(
    id_caja,
    id_usuario_cajero,
    accion_operativa,
    id_venta,
    detalle,
    fecha_hora,
    estado
)
SELECT
    ac.id_caja,
    u.id_usuario,
    N'COBRO',
    v.id_venta,
    CONCAT(
        N'Cobro de venta #',
        v.id_venta,
        N' por ₡',
        CAST(v.total_cobrado AS NVARCHAR(30)),
        N'. Método de pago: ',
        v.metodo_pago,
        N'.'
    ),
    v.fecha_hora,
    1
FROM dbo.Ventas v
INNER JOIN dbo.Apertura_Caja ac
    ON ac.id_apertura = v.id_apertura
INNER JOIN dbo.Empleados e
    ON e.id_empleado = v.id_empleado
INNER JOIN dbo.Usuarios u
    ON u.id_usuario = e.id_usuario
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Bitacora_PDV b
    WHERE b.id_venta = v.id_venta
      AND b.accion_operativa = N'COBRO'
);
GO


-- =====================================================================================
-- 4) BITACORA_FINANCIERA
--    Una entrada financiera por cada venta.
-- =====================================================================================
INSERT INTO dbo.Bitacora_Financiera
(
    id_usuario,
    tabla_afectada,
    id_registro_afectado,
    accion,
    valor_anterior,
    valor_nuevo,
    detalle,
    ip_origen,
    dispositivo,
    fecha_hora
)
SELECT
    u.id_usuario,
    N'Ventas',
    v.id_venta,

    CASE
        WHEN v.estado_venta = N'anulada' THEN N'ANULACION'
        ELSE N'COBRO'
    END,

    NULL,

    CONCAT(
        N'{"total_cobrado":',
        CAST(v.total_cobrado AS NVARCHAR(30)),
        N',"metodo_pago":"',
        v.metodo_pago,
        N'","estado_venta":"',
        v.estado_venta,
        N'"}'
    ),

    CASE
        WHEN v.estado_venta = N'anulada'
            THEN CONCAT(N'Venta #', v.id_venta, N' anulada.')
        ELSE
            CONCAT(
                N'Cobro registrado para venta #',
                v.id_venta,
                N' mediante ',
                v.metodo_pago,
                N'.'
            )
    END,

    CONCAT(N'192.168.20.', 20 + (v.id_venta % 25)),
    N'Terminal de caja',
    v.fecha_hora
FROM dbo.Ventas v
INNER JOIN dbo.Empleados e
    ON e.id_empleado = v.id_empleado
INNER JOIN dbo.Usuarios u
    ON u.id_usuario = e.id_usuario
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Bitacora_Financiera bf
    WHERE bf.tabla_afectada = N'Ventas'
      AND bf.id_registro_afectado = v.id_venta
      AND bf.accion IN (N'COBRO', N'ANULACION')
);
GO


-- =====================================================================================
-- 5) BITACORA_INVENTARIO
--    Un movimiento de salida asociado a cada pedido que tenga detalle.
--    La fecha se toma del pedido para mantener la distribución histórica.
--
--    IMPORTANTE:
--    Bitacora_Inventario no posee id_pedido ni id_insumo en tu diseño,
--    por eso se identifica el pedido dentro de "detalle" y de los valores JSON.
-- =====================================================================================
INSERT INTO dbo.Bitacora_Inventario
(
    id_usuario,
    accion,
    valor_anterior,
    valor_nuevo,
    detalle,
    ip_origen,
    dispositivo,
    fecha_hora
)
SELECT
    usuario_inv.id_usuario,
    N'salida',

    CONCAT(
        N'{"pedido":',
        p.id_pedido,
        N',"movimiento":"antes_consumo"}'
    ),

    CONCAT(
        N'{"pedido":',
        p.id_pedido,
        N',"movimiento":"consumo_registrado"}'
    ),

    CONCAT(
        N'Salida de inventario asociada al consumo del pedido #',
        p.id_pedido,
        N'. Productos registrados: ',
        COUNT(dp.id_detalle),
        N'.'
    ),

    N'192.168.20.45',
    N'Terminal de cocina',
    DATEADD(MINUTE, 10, p.fecha_hora)

FROM dbo.Pedidos p
INNER JOIN dbo.Detalle_Pedido dp
    ON dp.id_pedido = p.id_pedido

CROSS APPLY
(
    SELECT TOP 1 u.id_usuario
    FROM dbo.Usuarios u
    INNER JOIN dbo.Roles r
        ON r.id_rol = u.id_rol
    WHERE r.nombre_rol IN (N'Cocinero', N'Supervisor', N'Administrador')
      AND u.estado = 1
    ORDER BY
        CASE r.nombre_rol
            WHEN N'Cocinero' THEN 1
            WHEN N'Supervisor' THEN 2
            ELSE 3
        END,
        u.id_usuario
) usuario_inv

WHERE p.estado_pedido <> N'cancelado'
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.Bitacora_Inventario bi
    WHERE bi.detalle LIKE CONCAT(N'%pedido #', p.id_pedido, N'.%')
      AND bi.accion = N'salida'
)

GROUP BY
    usuario_inv.id_usuario,
    p.id_pedido,
    p.fecha_hora;
GO


-- =====================================================================================
-- 6) BITACORA_REPORTES
--    Un registro por cada reporte generado.
-- =====================================================================================
INSERT INTO dbo.Bitacora_Reportes
(
    id_usuario,
    accion,
    valor_anterior,
    valor_nuevo,
    detalle,
    ip_origen,
    dispositivo,
    fecha_hora
)
SELECT
    rg.id_usuario,
    N'GENERACION',
    NULL,

    CONCAT(
        N'{"reporte":"',
        rg.tipo_reporte,
        N'","formato":"',
        rg.formato_salida,
        N'"}'
    ),

    CONCAT(
        N'Se generó el reporte ',
        rg.tipo_reporte,
        N' en formato ',
        rg.formato_salida,
        N'.'
    ),

    CONCAT(N'192.168.20.', 50 + (rg.id_reporte % 10)),
    N'Equipo de supervisión',
    rg.fecha_generacion

FROM dbo.Reportes_Generados rg

WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Bitacora_Reportes br
    WHERE br.accion = N'GENERACION'
      AND br.fecha_hora = rg.fecha_generacion
      AND br.id_usuario = rg.id_usuario
);
GO


-- =====================================================================================
-- 7) BITACORA_REPORTES - DESCARGAS
--    Una parte de los reportes también registra descarga.
-- =====================================================================================
INSERT INTO dbo.Bitacora_Reportes
(
    id_usuario,
    accion,
    valor_anterior,
    valor_nuevo,
    detalle,
    ip_origen,
    dispositivo,
    fecha_hora
)
SELECT
    rg.id_usuario,
    N'DESCARGA',
    N'Reporte generado',
    CONCAT(N'Descargado en ', rg.formato_salida),

    CONCAT(
        N'Reporte ',
        rg.tipo_reporte,
        N' descargado por el usuario.'
    ),

    CONCAT(N'192.168.20.', 50 + (rg.id_reporte % 10)),
    N'Equipo de supervisión',
    DATEADD(MINUTE, 2 + (rg.id_reporte % 8), rg.fecha_generacion)

FROM dbo.Reportes_Generados rg

WHERE rg.id_reporte % 3 = 0
AND NOT EXISTS
(
    SELECT 1
    FROM dbo.Bitacora_Reportes br
    WHERE br.accion = N'DESCARGA'
      AND br.id_usuario = rg.id_usuario
      AND br.fecha_hora BETWEEN
          rg.fecha_generacion
          AND DATEADD(MINUTE, 10, rg.fecha_generacion)
);
GO


-- =====================================================================================
-- CONSULTA FINAL PARA REVISAR LAS CANTIDADES
-- =====================================================================================
SELECT N'Pedidos' AS Tabla, COUNT(*) AS Cantidad FROM dbo.Pedidos
UNION ALL
SELECT N'Ventas', COUNT(*) FROM dbo.Ventas
UNION ALL
SELECT N'Bitacora_Pedidos', COUNT(*) FROM dbo.Bitacora_Pedidos
UNION ALL
SELECT N'Bitacora_PDV', COUNT(*) FROM dbo.Bitacora_PDV
UNION ALL
SELECT N'Bitacora_Financiera', COUNT(*) FROM dbo.Bitacora_Financiera
UNION ALL
SELECT N'Bitacora_Inventario', COUNT(*) FROM dbo.Bitacora_Inventario
UNION ALL
SELECT N'Bitacora_Reportes', COUNT(*) FROM dbo.Bitacora_Reportes;
GO
