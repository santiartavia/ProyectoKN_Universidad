using AccesoADatos;
using Abstracciones.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Mesero", "Administrador" })]
    public class MeseroController : OperacionController
    {


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

            var mesas = db.Mesas.Where(m => m.Estado).OrderBy(m => m.NumeroMesa).ToList();
            ViewBag.Mesas = mesas;
            ViewBag.MesasOcupadasCount = mesas.Count(m => m.EstadoMesa == "ocupada");

            ViewBag.Detalles = db.DetallePedidos.Where(d => d.Estado == true).ToList();
            ViewBag.Productos = db.Productos.Where(p => p.Estado == true).ToList();

            ViewBag.HistorialEstados = db.HistorialEstadosPedido
                .Where(h => h.Estado == true)
                .OrderByDescending(h => h.FechaHoraCambio)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

            int usuarioActual = (int)Session["UsuarioId"];
            if (string.Equals(Session["RolNombre"] as string, "Mesero", StringComparison.OrdinalIgnoreCase) &&
                !db.Empleados.Any(e => e.IdEmpleado == pedido.IdEmpleado && e.IdUsuario == usuarioActual && e.Estado))
                throw new ArgumentException("Solo el mesero asignado puede entregar el pedido.");
            string estadoAnterior = pedido.EstadoPedido;
            int uid = Session["UsuarioId"] as int? ?? 0;
            if (uid <= 0)
            {
                uid = db.Usuarios.Where(u => u.Estado).Select(u => u.IdUsuario).FirstOrDefault();
            }

            var detalles = db.DetallePedidos.Where(d => d.IdPedido == idPedido && d.Estado).ToList();
            foreach (var d in detalles)
            {
                d.EstadoItem = "entregado";
            }

            if (pedido.TipoServicio == "rapida" && db.Ventas.Any(v => v.IdPedido == pedido.IdPedido && v.Estado && v.EstadoVenta == "completada"))
            {
                pedido.EstadoPedido = "finalizado";
                pedido.FechaHoraFinalizacion = DateTime.Now;

                RegistrarHistorial(
                    pedido.IdPedido,
                    estadoAnterior,
                    "finalizado",
                    Session["UsuarioNombre"]?.ToString() ?? "Mesero",
                    "Pedido rápido (venta ya pagada en PDV) entregado y finalizado directamente."
                );

                if (uid > 0)
                {
                    db.BitacoraPedidos.Add(new BitacoraPedido
                    {
                        IdUsuario = uid,
                        IdPedido = idPedido,
                        Accion = "ENTREGA",
                        EstadoAnterior = estadoAnterior,
                        EstadoNuevo = "finalizado",
                        Detalle = "Pedido rápido entregado; ya fue cobrado en el Punto de Venta, por lo que se finaliza sin pasar por caja.",
                        FechaHora = DateTime.Now
                    });
                }

                db.SaveChanges();
                TempData["Mensaje"] = "Pedido rápido entregado. Como la venta ya fue pagada en PDV, quedó finalizado.";
                return RedirectToAction("Index");
            }

            pedido.EstadoPedido = "entregado";
            pedido.FechaHoraEntrega = DateTime.Now;

            RegistrarHistorial(
                pedido.IdPedido,
                estadoAnterior,
                "entregado",
                Session["UsuarioNombre"]?.ToString() ?? "Mesero",
                "Mesero entregó el pedido al cliente"
            );

            if (uid > 0)
            {
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
            }

            db.SaveChanges();

            TempData["Mensaje"] = $"Pedido #{idPedido} entregado correctamente. Queda habilitado para pago en Caja.";
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
