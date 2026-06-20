using AccesoADatos;
using Abstracciones.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    public class PedidosController : Controller
    {
        private ColibriDbContext db = new ColibriDbContext();

        public ActionResult Index()
        {
            ViewBag.Productos = db.Productos
                .Where(p => p.Estado == true && p.Disponible == true)
                .ToList();

            ViewBag.Pedidos = db.Pedidos
                .Where(p => p.Estado == true)
                .OrderByDescending(p => p.FechaHora)
                .ToList();

            ViewBag.Detalles = db.DetallePedidos
                .Where(d => d.Estado == true)
                .ToList();

            ViewBag.Mesas = db.Mesas.ToList();

            return View();
        }

        [HttpPost]
        public ActionResult CrearOrden(int idMesa, int idMesero, byte comensales, int idProducto, decimal cantidad, string tipoServicio, string observaciones)
        {
            if (idMesa <= 0 || idMesero <= 0 || comensales <= 0 || idProducto <= 0 || cantidad <= 0)
            {
                TempData["Error"] = "Debe completar todos los campos obligatorios.";
                return RedirectToAction("Index");
            }

            var producto = db.Productos.FirstOrDefault(p => p.IdProducto == idProducto && p.Estado == true && p.Disponible == true);

            if (producto == null)
            {
                TempData["Error"] = "El producto seleccionado no existe o no está disponible.";
                return RedirectToAction("Index");
            }

            if (tipoServicio == "Salón" || tipoServicio == "Salon")
            {
                tipoServicio = "mesa";
            }

            if (tipoServicio == "Para llevar")
            {
                tipoServicio = "para_llevar";
            }

            if (tipoServicio == "Delivery")
            {
                tipoServicio = "delivery";
            }

            var pedido = new Pedido
            {
                IdMesa = idMesa,
                IdEmpleado = idMesero,
                TipoServicio = tipoServicio,
                CantidadComensales = comensales,
                EstadoPedido = "abierto",
                FechaHora = DateTime.Now,
                Observaciones = observaciones,
                Estado = true
            };

            db.Pedidos.Add(pedido);
            db.SaveChanges();

            var detalle = new DetallePedido
            {
                IdPedido = pedido.IdPedido,
                IdProducto = idProducto,
                Cantidad = cantidad,
                PrecioUnitario = producto.PrecioVenta,
                ObservacionesItem = observaciones,
                EstadoItem = "pendiente",
                Estado = true
            };

            db.DetallePedidos.Add(detalle);

            /*
            db.BitacoraPedidos.Add(new BitacoraPedido
            {
                IdUsuario = 1,
                IdPedido = pedido.IdPedido,
                Accion = "CREACION",
                EstadoAnterior = null,
                EstadoNuevo = "abierto",
                Detalle = "Orden creada desde seguimiento de pedidos",
                FechaHora = DateTime.Now
            });
            */

            db.SaveChanges();

            TempData["Mensaje"] = "Orden creada correctamente. Ya aparece en cocina.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AgregarProducto(int idPedido, int idProducto, decimal cantidad, string observacionesItem)
        {
            if (idPedido <= 0 || idProducto <= 0 || cantidad <= 0)
            {
                TempData["Error"] = "Debe seleccionar un pedido, un producto y una cantidad válida.";
                return RedirectToAction("Index");
            }

            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado == true);
            var producto = db.Productos.FirstOrDefault(p => p.IdProducto == idProducto && p.Estado == true && p.Disponible == true);

            if (pedido == null || producto == null)
            {
                TempData["Error"] = "El pedido o producto no existe.";
                return RedirectToAction("Index");
            }

            db.DetallePedidos.Add(new DetallePedido
            {
                IdPedido = idPedido,
                IdProducto = idProducto,
                Cantidad = cantidad,
                PrecioUnitario = producto.PrecioVenta,
                ObservacionesItem = observacionesItem,
                EstadoItem = "pendiente",
                Estado = true
            });

            /*
            db.BitacoraPedidos.Add(new BitacoraPedido
            {
                IdUsuario = 1,
                IdPedido = idPedido,
                Accion = "MODIFICACION",
                EstadoAnterior = pedido.EstadoPedido,
                EstadoNuevo = pedido.EstadoPedido,
                Detalle = "Producto agregado a la orden existente",
                FechaHora = DateTime.Now
            });
            */

            db.SaveChanges();

            TempData["Mensaje"] = "Producto agregado correctamente a la orden.";
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