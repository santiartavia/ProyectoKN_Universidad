using AccesoADatos;
using Abstracciones.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Cajero", "Administrador" })]
    public class CajeroController : Controller
    {
        private ColibriDbContext db = new ColibriDbContext();

        public ActionResult Index()
        {
            ViewBag.PedidosEntregados = db.Pedidos
                .Where(p => p.Estado == true && p.EstadoPedido == "entregado")
                .OrderByDescending(p => p.FechaHoraEntrega)
                .ToList();

            ViewBag.PedidosFinalizados = db.Pedidos
                .Where(p => p.Estado == true && p.EstadoPedido == "finalizado")
                .OrderByDescending(p => p.FechaHoraFinalizacion)
                .ToList();

            ViewBag.Detalles = db.DetallePedidos.Where(d => d.Estado == true).ToList();
            ViewBag.Productos = db.Productos.Where(p => p.Estado == true).ToList();
            ViewBag.Ventas = db.Ventas.Where(v => v.Estado == true).ToList();

            return View();
        }

        [HttpPost]
        public ActionResult RegistrarPago(int idPedido, string metodoPago, decimal montoRecibido)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado == true);

            if (pedido == null)
            {
                TempData["Error"] = "El pedido no existe.";
                return RedirectToAction("Index");
            }

            if (pedido.EstadoPedido != "entregado")
            {
                TempData["Error"] = "Solo se pueden cobrar pedidos entregados.";
                return RedirectToAction("Index");
            }

            metodoPago = NormalizarMetodoPago(metodoPago);

            decimal total = CalcularTotalPedido(idPedido);

            if (total <= 0)
            {
                TempData["Error"] = "El pedido no tiene productos para cobrar.";
                return RedirectToAction("Index");
            }

            if (montoRecibido < total)
            {
                TempData["Error"] = "El monto recibido no puede ser menor al total.";
                return RedirectToAction("Index");
            }

            var ventaExistente = db.Ventas.FirstOrDefault(v => v.IdPedido == idPedido && v.Estado == true && v.EstadoVenta == "completada");

            if (ventaExistente != null)
            {
                TempData["Error"] = "Este pedido ya tiene un pago registrado.";
                return RedirectToAction("Index");
            }

            var subcuenta = db.SubcuentasPedido.FirstOrDefault(s => s.IdPedido == idPedido && s.Estado == true);

            if (subcuenta == null)
            {
                subcuenta = new SubcuentaPedido
                {
                    IdPedido = idPedido,
                    NombreSubcuenta = "Cuenta principal",
                    Estado = true
                };

                db.SubcuentasPedido.Add(subcuenta);
                db.SaveChanges();
            }

            int idEmpleado = ObtenerIdEmpleado();

            var venta = new Venta
            {
                IdPedido = idPedido,
                IdSubcuenta = subcuenta.IdSubcuenta,
                IdEmpleado = idEmpleado,
                IdApertura = null,
                TipoVenta = "normal",
                TotalCobrado = total,
                MontoRecibido = montoRecibido,
                Vuelto = montoRecibido - total,
                MetodoPago = metodoPago,
                EstadoVenta = "completada",
                FechaHora = DateTime.Now,
                Estado = true
            };

            db.Ventas.Add(venta);

            int uid = Session["UsuarioId"] as int? ?? 0;
            db.BitacoraPedidos.Add(new BitacoraPedido
            {
                IdUsuario = uid,
                IdPedido = idPedido,
                Accion = "PAGO",
                Detalle = "Pago registrado: " + metodoPago + ", total " + total.ToString("N2"),
                FechaHora = DateTime.Now
            });

            db.SaveChanges();

            TempData["Mensaje"] = "Pago registrado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult FinalizarOrden(int idPedido)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado == true);

            if (pedido == null)
            {
                TempData["Error"] = "El pedido no existe.";
                return RedirectToAction("Index");
            }

            if (pedido.EstadoPedido != "entregado")
            {
                TempData["Error"] = "Solo se pueden finalizar pedidos entregados.";
                return RedirectToAction("Index");
            }

            var venta = db.Ventas.FirstOrDefault(v => v.IdPedido == idPedido && v.Estado == true && v.EstadoVenta == "completada");

            if (venta == null)
            {
                TempData["Error"] = "No se puede finalizar la orden porque no tiene pago registrado.";
                return RedirectToAction("Index");
            }

            string estadoAnterior = pedido.EstadoPedido;
            pedido.EstadoPedido = "finalizado";
            pedido.FechaHoraFinalizacion = DateTime.Now;

            db.HistorialEstadosPedido.Add(new HistorialEstadoPedido
            {
                IdPedido = idPedido,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = "finalizado",
                UsuarioResponsable = "Cajero",
                FechaHoraCambio = DateTime.Now,
                Detalle = "Cajero finalizo la orden y libero la mesa.",
                Estado = true
            });

            if (pedido.IdMesa.HasValue)
            {
                var mesa = db.Mesas.FirstOrDefault(m => m.IdMesa == pedido.IdMesa.Value);

                if (mesa != null)
                {
                    mesa.EstadoMesa = "disponible";
                }
            }

            db.SaveChanges();

            TempData["Mensaje"] = "Orden finalizada correctamente y mesa liberada.";
            return RedirectToAction("Index");
        }

        private decimal CalcularTotalPedido(int idPedido)
        {
            return db.DetallePedidos
                .Where(d => d.IdPedido == idPedido && d.Estado == true)
                .Select(d => d.Cantidad * d.PrecioUnitario)
                .DefaultIfEmpty(0)
                .Sum();
        }

        private string NormalizarMetodoPago(string metodoPago)
        {
            if (metodoPago == "Efectivo")
            {
                return "efectivo";
            }

            if (metodoPago == "Tarjeta")
            {
                return "tarjeta";
            }

            if (metodoPago == "SINPE")
            {
                return "sinpe";
            }

            if (metodoPago == "Mixto")
            {
                return "mixto";
            }

            return metodoPago;
        }

        private int ObtenerIdEmpleado()
        {
            var empleado = db.Empleados
                .Where(e => e.Estado == true)
                .OrderBy(e => e.IdEmpleado)
                .FirstOrDefault();

            if (empleado != null)
            {
                return empleado.IdEmpleado;
            }

            return 1;
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