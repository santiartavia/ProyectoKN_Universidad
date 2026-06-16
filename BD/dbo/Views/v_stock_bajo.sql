
CREATE   VIEW dbo.v_stock_bajo AS
SELECT
    i.id_insumo, i.nombre_insumo, c.nombre_categoria,
    i.unidad_medida, i.stock_minimo, i.stock_actual,
    i.stock_minimo - i.stock_actual AS unidades_faltantes
FROM dbo.Insumos i
JOIN dbo.Categorias_Insumo c ON c.id_categoria = i.id_categoria
WHERE i.stock_actual < i.stock_minimo AND i.estado = 1;