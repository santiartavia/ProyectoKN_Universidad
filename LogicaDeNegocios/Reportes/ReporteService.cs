using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;

namespace LogicaDeNegocios.Reportes
{
    public class ReporteService
    {
        public ReporteResultado Generar(string tipoReporte, DateTime? fi, DateTime? ff, ReporteFiltros filtros)
        {
            if (filtros == null) filtros = new ReporteFiltros();

            using (var ctx = new ColibriDbContext())
            {
                switch (tipoReporte)
                {
                    case "ventas":
                        return ReporteVentas(ctx, fi, ff, filtros);
                    case "productos_mas_vendidos":
                        return ReporteProductos(ctx, fi, ff, filtros);
                    case "ingresos_metodo_pago":
                        return ReporteIngresos(ctx, fi, ff, filtros);
                    case "desempenio_meseros":
                        return ReporteMeseros(ctx, fi, ff, filtros);
                    case "inventario":
                        return ReporteInventario(ctx, filtros);
                    case "egresos":
                        return ReporteEgresos(ctx, fi, ff, filtros);
                    case "cierre_turno":
                        return ReporteCierres(ctx, fi, ff);
                    default:
                        throw new ArgumentException($"Tipo de reporte '{tipoReporte}' no válido.");
                }
            }
        }

        private ReporteResultado ReporteVentas(ColibriDbContext ctx, DateTime? fi, DateTime? ff, ReporteFiltros f)
        {
            var q = ctx.Ventas.Where(v => v.Estado);
            if (fi.HasValue) q = q.Where(v => v.FechaHora >= fi.Value);
            if (ff.HasValue) q = q.Where(v => v.FechaHora <= ff.Value);
            if (!string.IsNullOrWhiteSpace(f.MetodoPago))
                q = q.Where(v => v.MetodoPago == f.MetodoPago);
            var data = q.OrderByDescending(v => v.FechaHora).ToList();

            var headers = new[] { "ID Venta", "ID Pedido", "Método de Pago", "Total Cobrado", "Vuelto", "Estado", "Fecha/Hora" };
            var rows = new List<string[]>();
            foreach (var v in data)
            {
                rows.Add(new[]
                {
                    v.IdVenta.ToString(),
                    v.IdPedido.ToString(),
                    EtiquetaMetodo(v.MetodoPago),
                    D(v.TotalCobrado),
                    D(v.Vuelto),
                    v.EstadoVenta ?? "Vacío",
                    v.FechaHora.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            if (data.Count > 0)
            {
                rows.Add(new string[0]);
                rows.Add(new[] { "", "", "TOTAL", D(data.Sum(x => x.TotalCobrado)), D(data.Sum(x => x.Vuelto)), data.Count + " ventas", "" });
                rows.Add(new string[0]);
                rows.Add(new[] { "DESGLOSE POR MÉTODO DE PAGO", "", "", "", "", "", "" });
                foreach (var g in data.GroupBy(v => EtiquetaMetodo(v.MetodoPago)).OrderBy(g => g.Key))
                    rows.Add(new[] { g.Key, g.Count() + " ventas", "", D(g.Sum(v => v.TotalCobrado)), "", "", "" });
            }

            return new ReporteResultado
            {
                Titulo = $"Reporte de Ventas ({Periodo(fi, ff)})",
                TipoReporte = "ventas",
                Headers = headers,
                Rows = rows,
                MensajeVacio = "No se encontraron ventas en el rango de fechas especificado."
            };
        }

        private ReporteResultado ReporteProductos(ColibriDbContext ctx, DateTime? fi, DateTime? ff, ReporteFiltros f)
        {
            var q = ctx.DetallePedidos.Where(d => d.Estado);
            if (fi.HasValue) q = q.Where(d => d.Pedido.FechaHora >= fi.Value);
            if (ff.HasValue) q = q.Where(d => d.Pedido.FechaHora <= ff.Value);
            if (f.CategoriaProducto.HasValue)
                q = q.Where(d => d.Producto.IdCategoriaProd == f.CategoriaProducto.Value);
            if (!string.IsNullOrWhiteSpace(f.TerminoBusqueda))
                q = q.Where(d => d.Producto.NombreProducto.Contains(f.TerminoBusqueda));

            int top = f.TopN.HasValue && f.TopN.Value > 0 ? f.TopN.Value : 10;
            var data = q
                .GroupBy(d => new { d.IdProducto, d.Producto.NombreProducto, d.Producto.IdCategoriaProd })
                .Select(g => new
                {
                    g.Key.IdProducto,
                    g.Key.NombreProducto,
                    g.Key.IdCategoriaProd,
                    Total = g.Sum(d => d.Cantidad),
                    Ingresos = g.Sum(d => d.Cantidad * d.PrecioUnitario)
                })
                .OrderByDescending(x => x.Total)
                .Take(top)
                .ToList();

            var cats = ctx.CategoriasProducto.ToDictionary(c => c.IdCategoriaProd, c => c.NombreCategoria);
            var headers = new[] { "#", "ID Producto", "Nombre", "Categoría", "Unidades Vendidas", "Ingresos Totales" };
            var rows = new List<string[]>();
            int i = 1;
            foreach (var p in data)
            {
                rows.Add(new[]
                {
                    i.ToString(),
                    p.IdProducto.ToString(),
                    p.NombreProducto ?? "Vacío",
                    cats.ContainsKey(p.IdCategoriaProd) ? cats[p.IdCategoriaProd] : "",
                    D(p.Total),
                    D(p.Ingresos)
                });
                i++;
            }

            if (data.Count > 0)
            {
                rows.Add(new string[0]);
                rows.Add(new[] { "", "", "TOTAL", "", D(data.Sum(x => x.Total)), D(data.Sum(x => x.Ingresos)) });
            }

            return new ReporteResultado
            {
                Titulo = $"Productos Más Vendidos - Top {top} ({Periodo(fi, ff)})",
                TipoReporte = "productos_mas_vendidos",
                Headers = headers,
                Rows = rows,
                MensajeVacio = "No se encontraron productos vendidos en el rango de fechas especificado."
            };
        }

        private ReporteResultado ReporteIngresos(ColibriDbContext ctx, DateTime? fi, DateTime? ff, ReporteFiltros f)
        {
            var q = ctx.Ventas.Where(v => v.Estado && v.EstadoVenta == "completada");
            if (fi.HasValue) q = q.Where(v => v.FechaHora >= fi.Value);
            if (ff.HasValue) q = q.Where(v => v.FechaHora <= ff.Value);
            if (!string.IsNullOrWhiteSpace(f.MetodoPago))
                q = q.Where(v => v.MetodoPago == f.MetodoPago);
            var data = q.ToList();

            var headers = new[] { "Método de Pago", "Cantidad de Ventas", "Total Ingresos" };
            var rows = new List<string[]>();
            foreach (var g in data.GroupBy(v => EtiquetaMetodo(v.MetodoPago)).OrderBy(g => g.Key))
                rows.Add(new[] { g.Key, g.Count().ToString(), D(g.Sum(v => v.TotalCobrado)) });

            if (data.Count > 0)
            {
                rows.Add(new string[0]);
                rows.Add(new[] { "TOTAL", data.Count.ToString(), D(data.Sum(v => v.TotalCobrado)) });
            }

            return new ReporteResultado
            {
                Titulo = $"Ingresos por Método de Pago ({Periodo(fi, ff)})",
                TipoReporte = "ingresos_metodo_pago",
                Headers = headers,
                Rows = rows,
                MensajeVacio = "No se encontraron ingresos en el rango de fechas especificado."
            };
        }

        private ReporteResultado ReporteMeseros(ColibriDbContext ctx, DateTime? fi, DateTime? ff, ReporteFiltros f)
        {
            var pedidosQ = ctx.Pedidos.Where(p => p.Estado && (p.EstadoPedido == "entregado" || p.EstadoPedido == "finalizado"));
            if (fi.HasValue) pedidosQ = pedidosQ.Where(p => p.FechaHora >= fi.Value);
            if (ff.HasValue) pedidosQ = pedidosQ.Where(p => p.FechaHora <= ff.Value);
            if (f.IdMesero.HasValue)
                pedidosQ = pedidosQ.Where(p => p.IdEmpleado == f.IdMesero.Value);

            var pedidos = pedidosQ
                .Select(p => new { p.IdPedido, p.IdEmpleado, p.FechaHora, p.FechaHoraEntrega, p.FechaHoraFinalizacion })
                .ToList();

            var pedidoVenta = ctx.Ventas.Where(v => v.EstadoVenta == "completada")
                .Select(v => new { v.IdPedido, v.TotalCobrado })
                .ToList();

            var empleados = ctx.Empleados.Where(e => e.Estado)
                .ToDictionary(e => e.IdEmpleado, e => $"{e.Nombre} {e.Apellidos}");

            var agrupado = pedidos
                .GroupBy(p => p.IdEmpleado)
                .Select(g => new
                {
                    IdEmpleado = g.Key,
                    PedidosAtendidos = g.Count(),
                    TotalIngresos = g.Sum(p => pedidoVenta.Where(v => v.IdPedido == p.IdPedido).Sum(v => v.TotalCobrado)),
                    TiempoMin = g
                        .Where(p => p.FechaHoraEntrega.HasValue || p.FechaHoraFinalizacion.HasValue)
                        .Select(p => (decimal)((p.FechaHoraFinalizacion ?? p.FechaHoraEntrega).Value - p.FechaHora).TotalMinutes)
                        .DefaultIfEmpty(0m)
                        .Average()
                })
                .OrderByDescending(x => x.PedidosAtendidos)
                .ToList();

            var headers = new[] { "ID Empleado", "Nombre", "Pedidos Atendidos", "Total Ingresos", "Tiempo Promedio (min)" };
            var rows = new List<string[]>();
            foreach (var m in agrupado)
            {
                rows.Add(new[]
                {
                    m.IdEmpleado.ToString(),
                    empleados.ContainsKey(m.IdEmpleado) ? empleados[m.IdEmpleado] : "Vacío",
                    m.PedidosAtendidos.ToString(),
                    D(m.TotalIngresos),
                    Math.Round(m.TiempoMin, 1).ToString("0.#", CultureInfo.InvariantCulture)
                });
            }

            return new ReporteResultado
            {
                Titulo = $"Desempeño de Meseros ({Periodo(fi, ff)})",
                TipoReporte = "desempenio_meseros",
                Headers = headers,
                Rows = rows,
                MensajeVacio = "No se encontraron pedidos atendidos en el rango de fechas especificado."
            };
        }

        private ReporteResultado ReporteInventario(ColibriDbContext ctx, ReporteFiltros f)
        {
            var q = ctx.Insumos.Include(i => i.Categoria).Where(i => i.Estado);
            if (f.CategoriaInsumo.HasValue)
                q = q.Where(i => i.IdCategoria == f.CategoriaInsumo.Value);
            if (f.StockBajo)
                q = q.Where(i => i.StockActual <= i.StockMinimo);
            if (!string.IsNullOrWhiteSpace(f.TerminoBusqueda))
                q = q.Where(i => i.NombreInsumo.Contains(f.TerminoBusqueda));
            var data = q.OrderBy(i => i.NombreInsumo).ToList();

            var proveedor = ctx.InsumosProveedores
                .Select(ip => new { ip.IdInsumo, Nombre = ip.Proveedor.NombreEmpresa })
                .GroupBy(x => x.IdInsumo)
                .ToDictionary(g => g.Key, g => g.First().Nombre);

            var headers = new[] { "ID Insumo", "Nombre", "Categoría", "Unidad", "Stock Actual", "Stock Mínimo", "Proveedor" };
            var rows = new List<string[]>();
            foreach (var i in data)
            {
                rows.Add(new[]
                {
                    i.IdInsumo.ToString(),
                    i.NombreInsumo ?? "Vacío",
                    i.Categoria != null ? i.Categoria.NombreCategoria : "",
                    i.UnidadMedida ?? "Vacío",
                    D(i.StockActual),
                    D(i.StockMinimo),
                    proveedor.ContainsKey(i.IdInsumo) ? proveedor[i.IdInsumo] : ""
                });
            }

            if (data.Count > 0)
            {
                rows.Add(new string[0]);
                rows.Add(new[] { "TOTAL", data.Count + " insumos", "", "", D(data.Sum(x => x.StockActual)), D(data.Sum(x => x.StockMinimo)), "" });
            }

            return new ReporteResultado
            {
                Titulo = "Reporte de Inventario",
                TipoReporte = "inventario",
                Headers = headers,
                Rows = rows,
                MensajeVacio = "No se encontraron insumos con los filtros seleccionados."
            };
        }

        private ReporteResultado ReporteEgresos(ColibriDbContext ctx, DateTime? fi, DateTime? ff, ReporteFiltros f)
        {
            var q = ctx.EgresosCaja.Where(e => e.Estado);
            if (fi.HasValue) q = q.Where(e => e.FechaHora >= fi.Value);
            if (ff.HasValue) q = q.Where(e => e.FechaHora <= ff.Value);
            if (!string.IsNullOrWhiteSpace(f.CategoriaEgreso))
                q = q.Where(e => e.CategoriaGasto == f.CategoriaEgreso);
            var data = q.OrderByDescending(e => e.FechaHora).ToList();

            var headers = new[] { "ID Egreso", "Categoría", "Descripción", "Monto", "Fecha/Hora" };
            var rows = new List<string[]>();
            foreach (var e in data)
            {
                rows.Add(new[]
                {
                    e.IdEgreso.ToString(),
                    e.CategoriaGasto ?? "Vacío",
                    e.Descripcion ?? "Vacío",
                    D(e.Monto),
                    e.FechaHora.ToString("yyyy-MM-dd HH:mm:ss")
                });
            }

            if (data.Count > 0)
            {
                rows.Add(new string[0]);
                rows.Add(new[] { "TOTAL", data.Count + " egresos", "", D(data.Sum(x => x.Monto)), "" });
                rows.Add(new string[0]);
                rows.Add(new[] { "TOTAL POR CATEGORÍA", "", "", "", "" });
                foreach (var g in data.GroupBy(e => e.CategoriaGasto ?? "Vacío").OrderByDescending(g => g.Sum(x => x.Monto)))
                    rows.Add(new[] { g.Key, g.Count() + " egresos", "", D(g.Sum(x => x.Monto)), "" });
            }

            return new ReporteResultado
            {
                Titulo = $"Reporte de Egresos ({Periodo(fi, ff)})",
                TipoReporte = "egresos",
                Headers = headers,
                Rows = rows,
                MensajeVacio = "No se encontraron egresos en el rango de fechas especificado."
            };
        }

        private ReporteResultado ReporteCierres(ColibriDbContext ctx, DateTime? fi, DateTime? ff)
        {
            var q = ctx.CierresCaja.Include(c => c.Cajero).Where(c => c.Estado);
            if (fi.HasValue) q = q.Where(c => c.FechaCierre >= fi.Value);
            if (ff.HasValue) q = q.Where(c => c.FechaCierre <= ff.Value);
            var data = q.OrderByDescending(c => c.FechaCierre).ToList();

            var headers = new[] { "ID Cierre", "Cajero", "Fecha", "Apertura", "Efectivo", "SINPE", "Tarjeta", "Egresos", "Esperado", "Real", "Descuadre" };
            var rows = new List<string[]>();
            foreach (var c in data)
            {
                rows.Add(new[]
                {
                    c.IdCierre.ToString(),
                    $"{(c.Cajero?.Nombre ?? "Vacío")} {(c.Cajero?.Apellidos ?? "Vacío")}",
                    c.FechaCierre.ToString("yyyy-MM-dd HH:mm"),
                    D(c.MontoApertura),
                    D(c.TotalEfectivo),
                    D(c.TotalSinpe),
                    D(c.TotalTarjeta),
                    D(c.TotalEgresos),
                    D(c.SaldoEsperado),
                    D(c.SaldoReal),
                    c.Descuadre ? "Sí" : "No"
                });
            }

            if (data.Count > 0)
            {
                rows.Add(new string[0]);
                rows.Add(new[]
                {
                    "TOTAL", "", "", "",
                    D(data.Sum(x => x.TotalEfectivo)),
                    D(data.Sum(x => x.TotalSinpe)),
                    D(data.Sum(x => x.TotalTarjeta)),
                    D(data.Sum(x => x.TotalEgresos)),
                    D(data.Sum(x => x.SaldoEsperado)),
                    D(data.Sum(x => x.SaldoReal)),
                    ""
                });
            }

            return new ReporteResultado
            {
                Titulo = $"Cierre de Turno ({Periodo(fi, ff)})",
                TipoReporte = "cierre_turno",
                Headers = headers,
                Rows = rows,
                MensajeVacio = "No se encontraron cierres de turno en el rango de fechas especificado."
            };
        }

        private string Periodo(DateTime? fi, DateTime? ff)
        {
            return $"{fi?.ToString("dd/MM/yyyy")} - {ff?.ToString("dd/MM/yyyy")}";
        }

        private string D(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);

        private string EtiquetaMetodo(string metodo)
        {
            if (string.IsNullOrWhiteSpace(metodo)) return "Sin método";
            switch (metodo.Trim().ToLowerInvariant())
            {
                case "efectivo":
                case "cash":
                    return "Efectivo";
                case "sinpe":
                    return "SINPE";
                case "tarjeta":
                case "card":
                    return "Tarjeta";
                case "mixto":
                case "mixta":
                    return "Mixto";
                default:
                    return metodo;
            }
        }
    }
}
