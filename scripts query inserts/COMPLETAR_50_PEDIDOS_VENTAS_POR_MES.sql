-- =================================================================================================
-- RESTAURANTE EL COLIBRI
-- COMPLEMENTO HISTORICO: MINIMO 50 PEDIDOS Y 50 VENTAS POR MES
-- Periodo: enero 2025 -> agosto 2026
--
-- IMPORTANTE:
--   * NO BORRA NADA.
--   * NO ACTUALIZA NADA.
--   * NO TOCA dbo.Roles.
--   * SOLO AGREGA LOS REGISTROS QUE HACEN FALTA PARA LLEGAR A 50 POR MES.
--   * NO AGREGA NADA A Bitacora_Reportes.
--   * Puede ejecutarse nuevamente: si un mes ya tiene 50 o más, no agrega pedidos/ventas extra.
-- =================================================================================================

USE [COLIBRI];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

-- -------------------------------------------------------------------------------------------------
-- 1. PEDIDOS
--    Completa cada mes hasta tener como mínimo 50 pedidos.
--    Los nuevos pedidos se distribuyen entre distintos días y horas.
-- -------------------------------------------------------------------------------------------------
;WITH Meses AS
(
    SELECT CAST('2025-01-01' AS DATE) AS InicioMes
    UNION ALL
    SELECT DATEADD(MONTH, 1, InicioMes)
    FROM Meses
    WHERE InicioMes < '2026-08-01'
),
Numeros AS
(
    SELECT n
    FROM (VALUES
        (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),
        (11),(12),(13),(14),(15),(16),(17),(18),(19),(20),
        (21),(22),(23),(24),(25),(26),(27),(28),(29),(30),
        (31),(32),(33),(34),(35),(36),(37),(38),(39),(40),
        (41),(42),(43),(44),(45),(46),(47),(48),(49),(50)
    ) N(n)
),
Faltantes AS
(
    SELECT
        m.InicioMes,
        CASE
            WHEN COUNT(p.id_pedido) >= 50 THEN 0
            ELSE 50 - COUNT(p.id_pedido)
        END AS CantidadFaltante
    FROM Meses m
    LEFT JOIN dbo.Pedidos p
        ON p.fecha_hora >= m.InicioMes
       AND p.fecha_hora < DATEADD(MONTH, 1, m.InicioMes)
    GROUP BY m.InicioMes
),
Datos AS
(
    SELECT
        f.InicioMes,
        n.n,
        DATEADD(
            MINUTE,
            ((n.n * 37) % 50),
            DATEADD(
                HOUR,
                CASE n.n % 7
                    WHEN 0 THEN 11
                    WHEN 1 THEN 12
                    WHEN 2 THEN 13
                    WHEN 3 THEN 14
                    WHEN 4 THEN 18
                    WHEN 5 THEN 19
                    ELSE 20
                END,
                CAST(
                    DATEADD(
                        DAY,
                        CASE
                            WHEN 2 + ((n.n * 5) % 25) >
                                 DAY(EOMONTH(f.InicioMes))
                            THEN DAY(EOMONTH(f.InicioMes)) - 1
                            ELSE 2 + ((n.n * 5) % 25)
                        END - 1,
                        f.InicioMes
                    ) AS DATETIME2
                )
            )
        ) AS FechaPedido
    FROM Faltantes f
    INNER JOIN Numeros n
        ON n.n <= f.CantidadFaltante
)
INSERT INTO dbo.Pedidos
(
    id_mesa,
    id_empleado,
    tipo_servicio,
    cantidad_comensales,
    estado_pedido,
    fecha_hora,
    observaciones,
    estado,
    fecha_hora_entrega,
    fecha_hora_finalizacion
)
SELECT
    CASE
        WHEN d.n % 5 IN (0,4) THEN NULL
        ELSE mesa.id_mesa
    END,

    mesero.id_empleado,

    CASE d.n % 10
        WHEN 0 THEN N'delivery'
        WHEN 4 THEN N'para_llevar'
        WHEN 8 THEN N'para_llevar'
        ELSE N'mesa'
    END,

    CASE
        WHEN d.n % 5 IN (0,4) THEN NULL
        ELSE CAST(1 + (d.n % 5) AS TINYINT)
    END,

    CASE
        WHEN d.n % 29 = 0 THEN N'cancelado'
        WHEN d.InicioMes = '2026-08-01' AND d.n % 17 = 0 THEN N'listo'
        WHEN d.InicioMes = '2026-08-01' AND d.n % 19 = 0 THEN N'en_proceso'
        ELSE N'finalizado'
    END,

    d.FechaPedido,

    CASE d.n % 9
        WHEN 0 THEN N'Salsa aparte'
        WHEN 1 THEN N'Sin cebolla en la ensalada'
        WHEN 2 THEN N'Cliente solicita factura electrónica'
        WHEN 3 THEN N'Bebida sin hielo'
        WHEN 4 THEN N'Entregar los platos al mismo tiempo'
        WHEN 5 THEN N'Poca sal'
        WHEN 6 THEN N'Sin observaciones especiales'
        WHEN 7 THEN N'Cliente solicita cubiertos adicionales'
        ELSE N'Empacar sobrante para llevar'
    END,

    CASE WHEN d.n % 29 = 0 THEN 0 ELSE 1 END,

    CASE
        WHEN d.n % 29 = 0 THEN NULL
        WHEN d.InicioMes = '2026-08-01' AND d.n % 17 = 0 THEN NULL
        WHEN d.InicioMes = '2026-08-01' AND d.n % 19 = 0 THEN NULL
        ELSE DATEADD(MINUTE, 28 + (d.n % 18), d.FechaPedido)
    END,

    CASE
        WHEN d.n % 29 = 0 THEN NULL
        WHEN d.InicioMes = '2026-08-01' AND d.n % 17 = 0 THEN NULL
        WHEN d.InicioMes = '2026-08-01' AND d.n % 19 = 0 THEN NULL
        ELSE DATEADD(MINUTE, 43 + (d.n % 22), d.FechaPedido)
    END

FROM Datos d

CROSS APPLY
(
    SELECT TOP 1 e.id_empleado
    FROM dbo.Empleados e
    INNER JOIN dbo.Usuarios u
        ON u.id_usuario = e.id_usuario
    INNER JOIN dbo.Roles r
        ON r.id_rol = u.id_rol
    WHERE r.nombre_rol = N'Mesero'
      AND e.estado = 1
      AND u.estado = 1
    ORDER BY ABS(CHECKSUM(CONCAT(d.InicioMes, N'-', d.n, N'-', e.id_empleado)))
) mesero

CROSS APPLY
(
    SELECT TOP 1 m.id_mesa
    FROM dbo.Mesas m
    WHERE m.estado = 1
    ORDER BY ABS(CHECKSUM(CONCAT(d.InicioMes, N'-', d.n, N'-', m.id_mesa)))
) mesa

OPTION (MAXRECURSION 100);
GO


-- -------------------------------------------------------------------------------------------------
-- 2. DETALLE PARA PEDIDOS NUEVOS
--    Agrega entre 1 y 3 líneas a los pedidos que todavía no tienen detalle.
-- -------------------------------------------------------------------------------------------------
;WITH Numeros AS
(
    SELECT n
    FROM (VALUES (1),(2),(3)) N(n)
)
INSERT INTO dbo.Detalle_Pedido
(
    id_pedido,
    id_producto,
    cantidad,
    precio_unitario,
    observaciones_item,
    estado_item,
    estado
)
SELECT
    p.id_pedido,
    prod.id_producto,
    CASE
        WHEN (p.id_pedido + n.n) % 13 = 0 THEN 2.00
        ELSE 1.00
    END,
    prod.precio_venta,

    CASE
        WHEN n.n = 1 AND p.id_pedido % 11 = 0 THEN N'Salsa aparte'
        WHEN n.n = 2 AND p.id_pedido % 14 = 0 THEN N'Sin cebolla'
        WHEN n.n = 3 THEN N'Producto adicional para compartir'
        ELSE NULL
    END,

    CASE p.estado_pedido
        WHEN N'cancelado' THEN N'cancelado'
        WHEN N'finalizado' THEN N'entregado'
        WHEN N'entregado' THEN N'entregado'
        WHEN N'listo' THEN N'listo'
        WHEN N'en_proceso' THEN N'preparando'
        ELSE N'pendiente'
    END,

    CASE WHEN p.estado_pedido = N'cancelado' THEN 0 ELSE 1 END

FROM dbo.Pedidos p

INNER JOIN Numeros n
    ON n.n <= CASE
        WHEN p.id_pedido % 4 = 0 THEN 3
        ELSE 2
    END

CROSS APPLY
(
    SELECT TOP 1
        pr.id_producto,
        pr.precio_venta
    FROM dbo.Productos pr
    WHERE pr.estado = 1
      AND pr.disponible = 1
    ORDER BY ABS(CHECKSUM(CONCAT(p.id_pedido, N'-', n.n, N'-', pr.id_producto)))
) prod

WHERE p.fecha_hora >= '2025-01-01'
  AND p.fecha_hora < '2026-09-01'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.Detalle_Pedido dp
      WHERE dp.id_pedido = p.id_pedido
  );
GO


-- -------------------------------------------------------------------------------------------------
-- 3. VENTAS VINCULADAS A PEDIDOS FINALIZADOS
--    Primero crea ventas para pedidos finalizados que todavía no tienen venta.
-- -------------------------------------------------------------------------------------------------
INSERT INTO dbo.Ventas
(
    id_pedido,
    id_subcuenta,
    id_empleado,
    id_apertura,
    tipo_venta,
    total_cobrado,
    monto_recibido,
    vuelto,
    metodo_pago,
    estado_venta,
    fecha_hora,
    estado
)
SELECT
    p.id_pedido,
    NULL,
    cajero.id_empleado,
    apertura.id_apertura,

    CASE p.id_pedido % 12
        WHEN 0 THEN N'descuento'
        ELSE N'normal'
    END,

    totales.TotalPedido,

    CASE
        WHEN p.id_pedido % 4 = 0
        THEN CEILING(totales.TotalPedido / 1000.0) * 1000
        ELSE totales.TotalPedido
    END,

    CASE
        WHEN p.id_pedido % 4 = 0
        THEN CEILING(totales.TotalPedido / 1000.0) * 1000 - totales.TotalPedido
        ELSE 0
    END,

    CASE p.id_pedido % 4
        WHEN 0 THEN N'efectivo'
        WHEN 1 THEN N'tarjeta'
        WHEN 2 THEN N'sinpe'
        ELSE N'mixto'
    END,

    N'completada',

    DATEADD(
        MINUTE,
        4 + (p.id_pedido % 8),
        ISNULL(p.fecha_hora_finalizacion, DATEADD(MINUTE, 50, p.fecha_hora))
    ),

    1

FROM dbo.Pedidos p

CROSS APPLY
(
    SELECT
        CAST(SUM(dp.cantidad * dp.precio_unitario) AS DECIMAL(12,2)) AS TotalPedido
    FROM dbo.Detalle_Pedido dp
    WHERE dp.id_pedido = p.id_pedido
      AND dp.estado = 1
) totales

CROSS APPLY
(
    SELECT TOP 1 e.id_empleado
    FROM dbo.Empleados e
    INNER JOIN dbo.Usuarios u
        ON u.id_usuario = e.id_usuario
    INNER JOIN dbo.Roles r
        ON r.id_rol = u.id_rol
    WHERE r.nombre_rol = N'Cajero'
      AND e.estado = 1
      AND u.estado = 1
    ORDER BY ABS(CHECKSUM(CONCAT(p.id_pedido, N'-', e.id_empleado)))
) cajero

OUTER APPLY
(
    SELECT TOP 1 a.id_apertura
    FROM dbo.Apertura_Caja a
    WHERE CAST(a.fecha_apertura AS DATE) = CAST(p.fecha_hora AS DATE)
    ORDER BY a.fecha_apertura DESC
) apertura

WHERE p.fecha_hora >= '2025-01-01'
  AND p.fecha_hora < '2026-09-01'
  AND p.estado_pedido IN (N'finalizado', N'entregado')
  AND totales.TotalPedido IS NOT NULL
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.Ventas v
      WHERE v.id_pedido = p.id_pedido
  );
GO


-- -------------------------------------------------------------------------------------------------
-- 4. VENTAS RAPIDAS COMPLEMENTARIAS
--    Después de agregar ventas de pedidos, vuelve a contar por mes.
--    Si todavía falta para llegar a 50, agrega solamente la diferencia.
-- -------------------------------------------------------------------------------------------------
;WITH Meses AS
(
    SELECT CAST('2025-01-01' AS DATE) AS InicioMes
    UNION ALL
    SELECT DATEADD(MONTH, 1, InicioMes)
    FROM Meses
    WHERE InicioMes < '2026-08-01'
),
Numeros AS
(
    SELECT n
    FROM (VALUES
        (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),
        (11),(12),(13),(14),(15),(16),(17),(18),(19),(20),
        (21),(22),(23),(24),(25),(26),(27),(28),(29),(30),
        (31),(32),(33),(34),(35),(36),(37),(38),(39),(40),
        (41),(42),(43),(44),(45),(46),(47),(48),(49),(50)
    ) N(n)
),
Faltantes AS
(
    SELECT
        m.InicioMes,
        CASE
            WHEN COUNT(v.id_venta) >= 50 THEN 0
            ELSE 50 - COUNT(v.id_venta)
        END AS CantidadFaltante
    FROM Meses m
    LEFT JOIN dbo.Ventas v
        ON v.fecha_hora >= m.InicioMes
       AND v.fecha_hora < DATEADD(MONTH, 1, m.InicioMes)
    GROUP BY m.InicioMes
),
Datos AS
(
    SELECT
        f.InicioMes,
        n.n,
        DATEADD(
            MINUTE,
            10 + ((n.n * 31) % 45),
            DATEADD(
                HOUR,
                CASE n.n % 6
                    WHEN 0 THEN 11
                    WHEN 1 THEN 12
                    WHEN 2 THEN 14
                    WHEN 3 THEN 18
                    WHEN 4 THEN 19
                    ELSE 20
                END,
                CAST(
                    DATEADD(
                        DAY,
                        CASE
                            WHEN 4 + ((n.n * 7) % 24) >
                                 DAY(EOMONTH(f.InicioMes))
                            THEN DAY(EOMONTH(f.InicioMes)) - 2
                            ELSE 4 + ((n.n * 7) % 24)
                        END - 1,
                        f.InicioMes
                    ) AS DATETIME2
                )
            )
        ) AS FechaVenta
    FROM Faltantes f
    INNER JOIN Numeros n
        ON n.n <= f.CantidadFaltante
)
INSERT INTO dbo.Ventas
(
    id_pedido,
    id_subcuenta,
    id_empleado,
    id_apertura,
    tipo_venta,
    total_cobrado,
    monto_recibido,
    vuelto,
    metodo_pago,
    estado_venta,
    fecha_hora,
    estado
)
SELECT
    NULL,
    NULL,

    cajero.id_empleado,

    apertura.id_apertura,

    N'rapida',

    prod.precio_venta,

    CASE
        WHEN d.n % 3 = 0
        THEN CEILING(prod.precio_venta / 1000.0) * 1000
        ELSE prod.precio_venta
    END,

    CASE
        WHEN d.n % 3 = 0
        THEN CEILING(prod.precio_venta / 1000.0) * 1000 - prod.precio_venta
        ELSE 0
    END,

    CASE d.n % 4
        WHEN 0 THEN N'efectivo'
        WHEN 1 THEN N'tarjeta'
        WHEN 2 THEN N'sinpe'
        ELSE N'mixto'
    END,

    N'completada',

    d.FechaVenta,

    1

FROM Datos d

CROSS APPLY
(
    SELECT TOP 1 e.id_empleado
    FROM dbo.Empleados e
    INNER JOIN dbo.Usuarios u
        ON u.id_usuario = e.id_usuario
    INNER JOIN dbo.Roles r
        ON r.id_rol = u.id_rol
    WHERE r.nombre_rol = N'Cajero'
      AND e.estado = 1
      AND u.estado = 1
    ORDER BY ABS(CHECKSUM(CONCAT(d.InicioMes, N'-', d.n, N'-', e.id_empleado)))
) cajero

CROSS APPLY
(
    SELECT TOP 1
        pr.id_producto,
        pr.precio_venta
    FROM dbo.Productos pr
    WHERE pr.estado = 1
      AND pr.disponible = 1
    ORDER BY ABS(CHECKSUM(CONCAT(d.InicioMes, N'-', d.n, N'-', pr.id_producto)))
) prod

OUTER APPLY
(
    SELECT TOP 1 a.id_apertura
    FROM dbo.Apertura_Caja a
    WHERE CAST(a.fecha_apertura AS DATE) = CAST(d.FechaVenta AS DATE)
    ORDER BY a.fecha_apertura DESC
) apertura

OPTION (MAXRECURSION 100);
GO


-- -------------------------------------------------------------------------------------------------
-- 5. DETALLE DE VENTA
--    Agrega detalle a cualquier venta del periodo que todavía no tenga detalle.
-- -------------------------------------------------------------------------------------------------
INSERT INTO dbo.Detalle_Venta
(
    id_venta,
    id_producto,
    cantidad,
    precio_unitario,
    subtotal_item,
    observaciones_item,
    estado
)
SELECT
    v.id_venta,
    prod.id_producto,
    1,
    prod.precio_venta,
    prod.precio_venta,
    N'Producto registrado como parte del consumo de la venta.',
    1
FROM dbo.Ventas v

CROSS APPLY
(
    SELECT TOP 1
        pr.id_producto,
        pr.precio_venta
    FROM dbo.Productos pr
    WHERE pr.estado = 1
      AND pr.disponible = 1
    ORDER BY ABS(CHECKSUM(CONCAT(v.id_venta, N'-', pr.id_producto)))
) prod

WHERE v.fecha_hora >= '2025-01-01'
  AND v.fecha_hora < '2026-09-01'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.Detalle_Venta dv
      WHERE dv.id_venta = v.id_venta
  );
GO


-- =================================================================================================
-- BITACORAS
-- NO SE TOCA Bitacora_Reportes.
-- =================================================================================================

-- -------------------------------------------------------------------------------------------------
-- 6. BITACORA_PEDIDOS - CREACION
--    Garantiza un evento de creación por cada pedido.
-- -------------------------------------------------------------------------------------------------
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
        WHEN N'mesa' THEN CONCAT(
            N'Pedido creado para mesa ',
            ISNULL(m.numero_mesa, N'sin asignar'),
            N'.'
        )
        WHEN N'para_llevar' THEN N'Pedido para llevar registrado.'
        WHEN N'delivery' THEN N'Pedido para entrega a domicilio registrado.'
        ELSE N'Pedido registrado.'
    END,

    CONCAT(N'192.168.20.', 30 + (p.id_pedido % 20)),
    N'Terminal de pedidos',
    p.fecha_hora,
    p.id_pedido

FROM dbo.Pedidos p
INNER JOIN dbo.Empleados e
    ON e.id_empleado = p.id_empleado
INNER JOIN dbo.Usuarios u
    ON u.id_usuario = e.id_usuario
LEFT JOIN dbo.Mesas m
    ON m.id_mesa = p.id_mesa

WHERE p.fecha_hora >= '2025-01-01'
  AND p.fecha_hora < '2026-09-01'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.Bitacora_Pedidos bp
      WHERE bp.id_pedido = p.id_pedido
        AND bp.accion = N'CREACION'
  );
GO


-- -------------------------------------------------------------------------------------------------
-- 7. BITACORA_PEDIDOS - CAMBIO/FINALIZACION
-- -------------------------------------------------------------------------------------------------
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
        WHEN p.estado_pedido = N'listo' THEN N'en_proceso'
        ELSE N'abierto'
    END,

    p.estado_pedido,

    CASE
        WHEN p.estado_pedido = N'cancelado'
            THEN N'Pedido cancelado durante la operación.'
        WHEN p.estado_pedido IN (N'finalizado', N'entregado')
            THEN N'Pedido entregado y finalizado correctamente.'
        WHEN p.estado_pedido = N'listo'
            THEN N'Cocina marcó el pedido como listo para entregar.'
        ELSE N'Pedido pasó a preparación.'
    END,

    CONCAT(N'192.168.20.', 30 + (p.id_pedido % 20)),
    N'Terminal de pedidos',

    CASE
        WHEN p.fecha_hora_finalizacion IS NOT NULL THEN p.fecha_hora_finalizacion
        WHEN p.fecha_hora_entrega IS NOT NULL THEN p.fecha_hora_entrega
        ELSE DATEADD(MINUTE, 25 + (p.id_pedido % 25), p.fecha_hora)
    END,

    p.id_pedido

FROM dbo.Pedidos p
INNER JOIN dbo.Empleados e
    ON e.id_empleado = p.id_empleado
INNER JOIN dbo.Usuarios u
    ON u.id_usuario = e.id_usuario

WHERE p.fecha_hora >= '2025-01-01'
  AND p.fecha_hora < '2026-09-01'
  AND p.estado_pedido IN
      (N'finalizado', N'entregado', N'cancelado', N'listo', N'en_proceso')
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.Bitacora_Pedidos bp
      WHERE bp.id_pedido = p.id_pedido
        AND bp.estado_nuevo = p.estado_pedido
  );
GO


-- -------------------------------------------------------------------------------------------------
-- 8. BITACORA_PDV
--    Garantiza al menos un evento de cobro por venta.
-- -------------------------------------------------------------------------------------------------
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
    caja.id_caja,
    u.id_usuario,
    N'COBRO',
    v.id_venta,

    CONCAT(
        N'Venta #',
        v.id_venta,
        N' cobrada por ₡',
        CAST(v.total_cobrado AS NVARCHAR(30)),
        N' mediante ',
        v.metodo_pago,
        N'.'
    ),

    v.fecha_hora,
    1

FROM dbo.Ventas v
INNER JOIN dbo.Empleados e
    ON e.id_empleado = v.id_empleado
INNER JOIN dbo.Usuarios u
    ON u.id_usuario = e.id_usuario

OUTER APPLY
(
    SELECT TOP 1 c.id_caja
    FROM dbo.Cajas c
    WHERE c.estado = 1
    ORDER BY
        CASE WHEN c.nombre_caja LIKE N'%Principal%' THEN 0 ELSE 1 END,
        c.id_caja
) caja

WHERE v.fecha_hora >= '2025-01-01'
  AND v.fecha_hora < '2026-09-01'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.Bitacora_PDV bp
      WHERE bp.id_venta = v.id_venta
        AND bp.accion_operativa = N'COBRO'
  );
GO


-- -------------------------------------------------------------------------------------------------
-- 9. BITACORA_FINANCIERA
--    Garantiza al menos un movimiento financiero por venta.
-- -------------------------------------------------------------------------------------------------
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
        N'{"venta":',
        v.id_venta,
        N',"total":',
        CAST(v.total_cobrado AS NVARCHAR(30)),
        N',"metodo":"',
        v.metodo_pago,
        N'"}'
    ),

    CONCAT(
        N'Movimiento financiero correspondiente a la venta #',
        v.id_venta,
        N'.'
    ),

    CONCAT(N'192.168.20.', 20 + (v.id_venta % 25)),
    N'Terminal de caja',
    v.fecha_hora

FROM dbo.Ventas v
INNER JOIN dbo.Empleados e
    ON e.id_empleado = v.id_empleado
INNER JOIN dbo.Usuarios u
    ON u.id_usuario = e.id_usuario

WHERE v.fecha_hora >= '2025-01-01'
  AND v.fecha_hora < '2026-09-01'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.Bitacora_Financiera bf
      WHERE bf.tabla_afectada = N'Ventas'
        AND bf.id_registro_afectado = v.id_venta
        AND bf.accion IN (N'COBRO', N'ANULACION')
  );
GO


-- -------------------------------------------------------------------------------------------------
-- 10. BITACORA_INVENTARIO
--     Un movimiento de consumo por pedido no cancelado.
-- -------------------------------------------------------------------------------------------------
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
    usuario_inventario.id_usuario,
    N'salida',

    CONCAT(
        N'{"pedido":',
        p.id_pedido,
        N',"estado":"antes_consumo"}'
    ),

    CONCAT(
        N'{"pedido":',
        p.id_pedido,
        N',"estado":"consumo_registrado"}'
    ),

    CONCAT(
        N'Salida de inventario asociada al pedido #',
        p.id_pedido,
        N'.'
    ),

    N'192.168.20.45',
    N'Terminal de cocina',
    DATEADD(MINUTE, 10 + (p.id_pedido % 12), p.fecha_hora)

FROM dbo.Pedidos p

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
) usuario_inventario

WHERE p.fecha_hora >= '2025-01-01'
  AND p.fecha_hora < '2026-09-01'
  AND p.estado_pedido <> N'cancelado'
  AND EXISTS
  (
      SELECT 1
      FROM dbo.Detalle_Pedido dp
      WHERE dp.id_pedido = p.id_pedido
  )
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.Bitacora_Inventario bi
      WHERE bi.detalle = CONCAT(
          N'Salida de inventario asociada al pedido #',
          p.id_pedido,
          N'.'
      )
  );
GO


-- =================================================================================================
-- VERIFICACION
-- Estas consultas NO modifican datos.
-- Deben mostrar por lo menos 50 pedidos y 50 ventas en cada mes.
-- =================================================================================================

SELECT
    YEAR(fecha_hora) AS Anio,
    MONTH(fecha_hora) AS Mes,
    COUNT(*) AS Pedidos
FROM dbo.Pedidos
WHERE fecha_hora >= '2025-01-01'
  AND fecha_hora < '2026-09-01'
GROUP BY YEAR(fecha_hora), MONTH(fecha_hora)
ORDER BY Anio, Mes;
GO

SELECT
    YEAR(fecha_hora) AS Anio,
    MONTH(fecha_hora) AS Mes,
    COUNT(*) AS Ventas
FROM dbo.Ventas
WHERE fecha_hora >= '2025-01-01'
  AND fecha_hora < '2026-09-01'
GROUP BY YEAR(fecha_hora), MONTH(fecha_hora)
ORDER BY Anio, Mes;
GO

SELECT
    YEAR(fecha_hora) AS Anio,
    MONTH(fecha_hora) AS Mes,
    COUNT(*) AS BitacoraPedidos
FROM dbo.Bitacora_Pedidos
WHERE fecha_hora >= '2025-01-01'
  AND fecha_hora < '2026-09-01'
GROUP BY YEAR(fecha_hora), MONTH(fecha_hora)
ORDER BY Anio, Mes;
GO

SELECT
    YEAR(fecha_hora) AS Anio,
    MONTH(fecha_hora) AS Mes,
    COUNT(*) AS BitacoraPDV
FROM dbo.Bitacora_PDV
WHERE fecha_hora >= '2025-01-01'
  AND fecha_hora < '2026-09-01'
GROUP BY YEAR(fecha_hora), MONTH(fecha_hora)
ORDER BY Anio, Mes;
GO

SELECT
    YEAR(fecha_hora) AS Anio,
    MONTH(fecha_hora) AS Mes,
    COUNT(*) AS BitacoraFinanciera
FROM dbo.Bitacora_Financiera
WHERE fecha_hora >= '2025-01-01'
  AND fecha_hora < '2026-09-01'
GROUP BY YEAR(fecha_hora), MONTH(fecha_hora)
ORDER BY Anio, Mes;
GO

SELECT
    YEAR(fecha_hora) AS Anio,
    MONTH(fecha_hora) AS Mes,
    COUNT(*) AS BitacoraInventario
FROM dbo.Bitacora_Inventario
WHERE fecha_hora >= '2025-01-01'
  AND fecha_hora < '2026-09-01'
GROUP BY YEAR(fecha_hora), MONTH(fecha_hora)
ORDER BY Anio, Mes;
GO

-- Bitacora_Reportes se dejó completamente intacta.
