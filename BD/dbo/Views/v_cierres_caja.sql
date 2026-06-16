
CREATE   VIEW dbo.v_cierres_caja AS
SELECT
    cc.id_cierre, cc.id_cajero, cc.id_apertura, ac.fecha_apertura,
    cc.fecha_cierre, cc.monto_apertura, cc.total_efectivo,
    cc.total_sinpe, cc.total_tarjeta, cc.total_egresos,
    cc.saldo_esperado, cc.saldo_real,
    cc.saldo_real - cc.saldo_esperado AS monto_diferencia,
    cc.descuadre, cc.estado
FROM dbo.Cierres_Caja cc
JOIN dbo.Apertura_Caja ac ON ac.id_apertura = cc.id_apertura;