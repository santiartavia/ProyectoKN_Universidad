using AccesoADatos;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    public class CocinaController : Controller
    {
        private ColibriDbContext db = new ColibriDbContext();

        public ActionResult Index()
        {
            var pedidos = db.Pedidos
                .Where(p => p.Estado == true &&
                       (p.EstadoPedido == "abierto" ||
                        p.EstadoPedido == "en_proceso" ||
                        p.EstadoPedido == "listo"))
                .OrderBy(p => p.FechaHora)
                .ToList();

            ViewBag.PedidosNuevos = pedidos.Count(p => p.EstadoPedido == "abierto");
            ViewBag.EnPreparacion = pedidos.Count(p => p.EstadoPedido == "en_proceso");
            ViewBag.PorEntregar = pedidos.Count(p => p.EstadoPedido == "listo");
            ViewBag.Completados = pedidos.Count(p => p.EstadoPedido == "entregado");

            ViewBag.Mesas = db.Mesas.ToList();
            ViewBag.Detalles = db.DetallePedidos.Where(d => d.Estado == true).ToList();
            ViewBag.Productos = db.Productos.ToList();

            return View(pedidos);
        }

        [HttpPost]
        public ActionResult IniciarPreparacion(int idPedido)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido);

            if (pedido != null)
            {
                pedido.EstadoPedido = "en_proceso";

                var detalles = db.DetallePedidos
                    .Where(d => d.IdPedido == idPedido && d.Estado == true && d.EstadoItem == "pendiente")
                    .ToList();

                foreach (var item in detalles)
                {
                    item.EstadoItem = "preparando";
                }

                db.SaveChanges();

                TempData["Mensaje"] = "El pedido pasó a preparación correctamente.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult MarcarItemPreparando(int idDetalle)
        {
            var detalle = db.DetallePedidos.FirstOrDefault(d => d.IdDetalle == idDetalle);

            if (detalle != null)
            {
                detalle.EstadoItem = "preparando";

                var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == detalle.IdPedido);
                if (pedido != null && pedido.EstadoPedido == "abierto")
                {
                    pedido.EstadoPedido = "en_proceso";
                }

                db.SaveChanges();

                TempData["Mensaje"] = "Producto marcado en preparación.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult MarcarItemListo(int idDetalle)
        {
            var detalle = db.DetallePedidos.FirstOrDefault(d => d.IdDetalle == idDetalle);

            if (detalle != null)
            {
                detalle.EstadoItem = "listo";

                var detallesPedido = db.DetallePedidos
                    .Where(d => d.IdPedido == detalle.IdPedido && d.Estado == true)
                    .ToList();

                if (detallesPedido.All(d => d.EstadoItem == "listo"))
                {
                    var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == detalle.IdPedido);

                    if (pedido != null)
                    {
                        pedido.EstadoPedido = "listo";
                    }
                }

                db.SaveChanges();

                TempData["Mensaje"] = "Producto marcado como listo.";
            }

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}