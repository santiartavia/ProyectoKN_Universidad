using AccesoADatos;
using Abstracciones.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Cocinero", "Administrador" })]
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
            ViewBag.Completados = db.Pedidos.Count(p => p.Estado == true && (p.EstadoPedido == "entregado" || p.EstadoPedido == "finalizado"));

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
                string estadoAnterior = pedido.EstadoPedido;

                pedido.EstadoPedido = "en_proceso";

                var detalles = db.DetallePedidos
                    .Where(d => d.IdPedido == idPedido && d.Estado == true && d.EstadoItem == "pendiente")
                    .ToList();

                foreach (var item in detalles)
                {
                    item.EstadoItem = "preparando";
                }

                RegistrarHistorial(
                    pedido.IdPedido,
                    estadoAnterior,
                    "en_proceso",
                    Session["UsuarioNombre"]?.ToString() ?? "Cocinero",
                    "Cocina inició la preparación del pedido"
                );

                db.SaveChanges();

                TempData["Mensaje"] = $"El pedido #{idPedido} pasó a preparación correctamente.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult MarcarPedidoListo(int idPedido)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado);
            if (pedido != null)
            {
                string estadoAnterior = pedido.EstadoPedido;
                pedido.EstadoPedido = "listo";

                var detalles = db.DetallePedidos.Where(d => d.IdPedido == idPedido && d.Estado).ToList();
                foreach (var item in detalles)
                {
                    item.EstadoItem = "listo";
                }

                RegistrarHistorial(
                    pedido.IdPedido,
                    estadoAnterior,
                    "listo",
                    Session["UsuarioNombre"]?.ToString() ?? "Cocinero",
                    "Todos los productos del pedido fueron marcados como listos por cocina"
                );

                db.SaveChanges();
                TempData["Mensaje"] = $"El pedido #{idPedido} fue marcado como Listo para servir.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult MarcarItemPreparando(int idDetalle)
        {
            var detalle = db.DetallePedidos.FirstOrDefault(d => d.IdDetalle == idDetalle);

            if (detalle != null)
            {
                string estadoItemAnterior = detalle.EstadoItem;
                detalle.EstadoItem = "preparando";

                var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == detalle.IdPedido);

                if (pedido != null && pedido.EstadoPedido == "abierto")
                {
                    string estadoPedidoAnterior = pedido.EstadoPedido;
                    pedido.EstadoPedido = "en_proceso";

                    RegistrarHistorial(
                        pedido.IdPedido,
                        estadoPedidoAnterior,
                        "en_proceso",
                        Session["UsuarioNombre"]?.ToString() ?? "Cocinero",
                        "Producto marcado en preparación y pedido pasó a en proceso"
                    );
                }
                else if (pedido != null)
                {
                    RegistrarHistorial(
                        pedido.IdPedido,
                        estadoItemAnterior,
                        "preparando",
                        Session["UsuarioNombre"]?.ToString() ?? "Cocinero",
                        "Producto marcado en preparación"
                    );
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
                string estadoItemAnterior = detalle.EstadoItem;
                detalle.EstadoItem = "listo";

                RegistrarHistorial(
                    detalle.IdPedido,
                    estadoItemAnterior,
                    "listo",
                    Session["UsuarioNombre"]?.ToString() ?? "Cocinero",
                    "Producto marcado como listo"
                );

                var detallesPedido = db.DetallePedidos
                    .Where(d => d.IdPedido == detalle.IdPedido && d.Estado == true)
                    .ToList();

                if (detallesPedido.All(d => d.EstadoItem == "listo"))
                {
                    var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == detalle.IdPedido);

                    if (pedido != null)
                    {
                        string estadoPedidoAnterior = pedido.EstadoPedido;
                        pedido.EstadoPedido = "listo";

                        RegistrarHistorial(
                            pedido.IdPedido,
                            estadoPedidoAnterior,
                            "listo",
                            Session["UsuarioNombre"]?.ToString() ?? "Cocinero",
                            "Todos los productos del pedido están listos"
                        );
                    }
                }

                db.SaveChanges();

                TempData["Mensaje"] = "Producto marcado como listo.";
            }

            return RedirectToAction("Index");
        }

        private void RegistrarHistorial(int idPedido, string estadoAnterior, string estadoNuevo, string usuarioResponsable, string detalle)
        {
            db.HistorialEstadosPedido.Add(new HistorialEstadoPedido
            {
                IdPedido = idPedido,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = estadoNuevo,
                UsuarioResponsable = usuarioResponsable,
                FechaHoraCambio = DateTime.Now,
                Detalle = detalle,
                Estado = true
            });
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
