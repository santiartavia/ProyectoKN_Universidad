using AccesoADatos;
using Abstracciones.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Mesero", "Administrador" })]
    public class MeseroController : Controller
    {
        private ColibriDbContext db = new ColibriDbContext();

        public ActionResult Index()
        {
            ViewBag.PedidosListos = db.Pedidos
                .Where(p => p.Estado == true && p.EstadoPedido == "listo")
                .OrderByDescending(p => p.FechaHora)
                .ToList();

            ViewBag.PedidosEntregados = db.Pedidos
                .Where(p => p.Estado == true && p.EstadoPedido == "entregado")
                .OrderByDescending(p => p.FechaHora)
                .ToList();

            ViewBag.PedidosSeguimiento = db.Pedidos
                .Where(p => p.Estado == true &&
                       (p.EstadoPedido == "abierto" ||
                        p.EstadoPedido == "en_proceso" ||
                        p.EstadoPedido == "listo" ||
                        p.EstadoPedido == "entregado" ||
                        p.EstadoPedido == "finalizado" ||
                        p.EstadoPedido == "cancelado"))
                .OrderByDescending(p => p.FechaHora)
                .ToList();

            ViewBag.Detalles = db.DetallePedidos.Where(d => d.Estado == true).ToList();
            ViewBag.Productos = db.Productos.Where(p => p.Estado == true).ToList();

            ViewBag.HistorialEstados = db.HistorialEstadosPedido
                .Where(h => h.Estado == true)
                .OrderByDescending(h => h.FechaHoraCambio)
                .ToList();

            return View();
        }

        [HttpPost]
        public ActionResult EntregarPedido(int idPedido)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado == true);

            if (pedido == null)
            {
                TempData["Error"] = "El pedido no existe.";
                return RedirectToAction("Index");
            }

            if (pedido.EstadoPedido != "listo")
            {
                TempData["Error"] = "Solo se pueden entregar pedidos en estado listo.";
                return RedirectToAction("Index");
            }

            string estadoAnterior = pedido.EstadoPedido;

            pedido.EstadoPedido = "entregado";
            pedido.FechaHoraEntrega = DateTime.Now;

            RegistrarHistorial(
                pedido.IdPedido,
                estadoAnterior,
                "entregado",
                "Mesero",
                "Mesero entrego el pedido al cliente"
            );

            int uid = Session["UsuarioId"] as int? ?? 0;
            db.BitacoraPedidos.Add(new BitacoraPedido
            {
                IdUsuario = uid,
                IdPedido = idPedido,
                Accion = "ENTREGA",
                EstadoAnterior = "listo",
                EstadoNuevo = "entregado",
                Detalle = "Pedido entregado por mesero.",
                FechaHora = DateTime.Now
            });

            db.SaveChanges();

            TempData["Mensaje"] = "Pedido entregado correctamente. Queda habilitado para pago.";
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