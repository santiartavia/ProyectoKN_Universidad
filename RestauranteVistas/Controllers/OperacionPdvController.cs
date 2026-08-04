using AccesoADatos;
using Abstracciones.Models;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Cajero", "Administrador" })]
    public class OperacionPdvController : Controller
    {
        private readonly ColibriDbContext db = new ColibriDbContext();

        public ActionResult Index(string estado, int? idMesa) { return View(CrearModelo(estado, idMesa)); }

        public JsonResult Resumen(string estado, int? idMesa)
        {
            var vm = CrearModelo(estado, idMesa);
            return Json(new
            {
                vm.VentasActivas,
                vm.VentasCobradas,
                vm.TotalCobrado,
                vm.TotalPendiente,
                vm.TiempoPromedioMinutos,
                Ventas = vm.Ventas.Select(v => new
                {
                    v.IdVenta,
                    v.Estado,
                    v.Mesa,
                    v.Total,
                    FechaHora = v.FechaHora.ToString("dd/MM/yyyy HH:mm"),
                    v.Demorada,
                    v.MuyEditada,
                    v.Ediciones
                }),
                Productos = vm.ProductosMasVendidos.Select(p => new { p.Nombre, p.Cantidad })
            }, JsonRequestBehavior.AllowGet);
        }

        private OperacionPdvViewModel CrearModelo(string estado, int? idMesa)
        {
            var inicio = DateTime.Today;
            var ventasTodas = db.Ventas.Where(v => v.Estado && v.FechaHora >= inicio).ToList();
            var detalles = db.DetallesVenta.Where(d => d.Estado).ToList();

            var idsPedidos = ventasTodas.Select(v => v.IdPedido).Distinct().ToList();
            var pedidosMap = db.Pedidos.Where(p => idsPedidos.Contains(p.IdPedido)).ToList();
            var mesasMap = db.Mesas.ToDictionary(m => m.IdMesa, m => m.NumeroMesa);

            var ventas = ventasTodas;
            if (!string.IsNullOrWhiteSpace(estado))
            {
                ventas = ventas.Where(v => v.EstadoVenta == estado).ToList();
            }
            if (idMesa.HasValue)
            {
                var idsPedidosMesa = pedidosMap
                    .Where(p => p.IdMesa == idMesa.Value)
                    .Select(p => p.IdPedido)
                    .ToHashSet();
                ventas = ventas.Where(v => v.IdPedido.HasValue && idsPedidosMesa.Contains(v.IdPedido.Value)).ToList();
            }

            var activas = ventas.Where(v => v.EstadoVenta == "abierta" || v.EstadoVenta == "en_revision" || v.EstadoVenta == "validada").ToList();
            var cobradas = ventas.Where(v => v.EstadoVenta == "completada").ToList();
            var ids = ventas.Select(v => v.IdVenta).ToList();

            var detallePedidos = db.DetallePedidos.Where(dp => dp.Estado && idsPedidos.Contains(dp.IdPedido)).ToList();

            decimal TotalVenta(Venta v)
            {
                if (v.TipoVenta == "rapida")
                    return detalles.Where(d => d.IdVenta == v.IdVenta).Sum(d => d.SubtotalItem);
                if (v.IdPedido.HasValue)
                    return detallePedidos.Where(dp => dp.IdPedido == v.IdPedido.Value).Sum(dp => dp.Cantidad * dp.PrecioUnitario);
                return v.TotalCobrado;
            }

            var auditoria = db.BitacoraPdv
                .Where(b => b.IdVenta != null && ids.Contains(b.IdVenta.Value))
                .ToList();

            var duraciones = cobradas.Select(v =>
            {
                var apertura = auditoria.Where(b => b.IdVenta == v.IdVenta && b.AccionOperativa == "AperturaVenta").OrderBy(b => b.FechaHora).FirstOrDefault();
                var cobro = auditoria.Where(b => b.IdVenta == v.IdVenta && b.AccionOperativa == "CobroRapido").OrderByDescending(b => b.FechaHora).FirstOrDefault();
                return apertura != null && cobro != null ? (decimal?)(cobro.FechaHora - apertura.FechaHora).TotalMinutes : null;
            }).Where(d => d.HasValue).Select(d => d.Value).ToList();

            return new OperacionPdvViewModel
            {
                VentasActivas = activas.Count,
                VentasCobradas = cobradas.Count,
                TotalCobrado = cobradas.Sum(v => v.TotalCobrado),
                TotalPendiente = activas.Sum(v => TotalVenta(v)),
                TiempoPromedioMinutos = duraciones.Any() ? Math.Round(duraciones.Average(), 1) : 0,
                Ventas = ventas.OrderByDescending(v => v.FechaHora).Select(v =>
                {
                    var ediciones = auditoria.Count(b => b.IdVenta == v.IdVenta && b.AccionOperativa == "EdicionVenta");
                    var pedido = v.IdPedido.HasValue ? pedidosMap.FirstOrDefault(p => p.IdPedido == v.IdPedido.Value) : null;
                    string mesa = null;
                    if (v.TipoVenta == "rapida")
                        mesa = "Venta rápida";
                    else if (pedido != null && pedido.IdMesa.HasValue && mesasMap.ContainsKey(pedido.IdMesa.Value))
                        mesa = "Mesa " + mesasMap[pedido.IdMesa.Value];
                    return new OperacionPdvViewModel.VentaOperacion
                    {
                        IdVenta = v.IdVenta,
                        Estado = v.EstadoVenta,
                        Mesa = mesa,
                        Total = TotalVenta(v),
                        FechaHora = v.FechaHora,
                        Demorada = v.EstadoVenta != "completada" && (DateTime.Now - v.FechaHora).TotalMinutes >= 20,
                        Ediciones = ediciones,
                        MuyEditada = ediciones >= 3,
                        IdPedido = v.IdPedido
                    };
                }).ToList(),
                FiltroEstado = estado,
                FiltroIdMesa = idMesa,
                Mesas = db.Mesas.Where(m => m.Estado).OrderBy(m => m.NumeroMesa).ToList(),
                ProductosMasVendidos = detalles
                    .Where(d => ids.Contains(d.IdVenta))
                    .GroupBy(d => d.IdProducto)
                    .Select(g => new { IdProducto = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                    .Concat(detallePedidos
                        .GroupBy(dp => dp.IdProducto)
                        .Select(g => new { IdProducto = g.Key, Cantidad = (int)g.Sum(x => x.Cantidad) }))
                    .GroupBy(x => x.IdProducto)
                    .Select(g => new { IdProducto = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
                    .OrderByDescending(x => x.Cantidad)
                    .Take(5)
                    .ToList()
                    .Select(g => new OperacionPdvViewModel.ProductoOperacion
                    {
                        Nombre = db.Productos.Where(p => p.IdProducto == g.IdProducto).Select(p => p.NombreProducto).FirstOrDefault() ?? ("Producto #" + g.IdProducto),
                        Cantidad = g.Cantidad
                    }).ToList()
            };
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}