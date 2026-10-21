using AccesoADatos;
using Abstracciones.Models;
using LogicaDeNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Mesero", "Administrador", "Cajero" })]
    public class PedidosController : OperacionController
    {


        public ActionResult Index()
        {
            ViewBag.Productos = db.Productos
                .Where(p => p.Estado == true && p.Disponible == true)
                .OrderBy(p => p.NombreProducto)
                .ToList();

            var pedidos = db.Pedidos
                .Where(p => p.Estado == true)
                .OrderByDescending(p => p.FechaHora)
                .ToList();
            ViewBag.Pedidos = pedidos;

            ViewBag.Detalles = db.DetallePedidos
                .Where(d => d.Estado == true)
                .ToList();

            // Mesas sin pedido activo en curso
            var mesasConPedidoActivo = pedidos
                .Where(p => p.IdMesa.HasValue && p.EstadoPedido != "cancelado" && p.EstadoPedido != "finalizado")
                .Select(p => p.IdMesa.Value)
                .Distinct()
                .ToList();

            ViewBag.Mesas = db.Mesas
                .Where(m => m.Estado && m.EstadoMesa != "inactiva" && !mesasConPedidoActivo.Contains(m.IdMesa))
                .OrderBy(m => m.NumeroMesa)
                .ToList();

            var todasMesas = db.Mesas.Where(m => m.Estado).ToList();
            ViewBag.NombresMesas = todasMesas.GroupBy(m => m.IdMesa).ToDictionary(g => g.Key, g => g.First().NumeroMesa);

            var empleados = db.Empleados.Include(e => e.Usuario).Include(e => e.Usuario.Rol)
                .Where(e => e.Estado && e.Usuario.Estado && e.Usuario.Rol.Estado && e.Usuario.Rol.NombreRol == "Mesero")
                .OrderByDescending(e => e.Usuario != null && e.Usuario.Rol != null && e.Usuario.Rol.NombreRol == "Mesero")
                .ThenBy(e => e.Nombre)
                .ToList();
            ViewBag.Meseros = empleados;
            ViewBag.NombresEmpleados = empleados.GroupBy(e => e.IdEmpleado).ToDictionary(g => g.Key, g => $"{g.First().Nombre} {g.First().Apellidos}");

            var uid = Session["UsuarioId"] as int? ?? 0;
            var empleado = empleados.FirstOrDefault(e => e.IdUsuario == uid);
            ViewBag.MeseroId = empleado?.IdEmpleado ?? 0;
            ViewBag.MeseroNombre = empleado != null ? empleado.Nombre + " " + empleado.Apellidos : (Session["UsuarioNombre"]?.ToString() ?? "");

            // KPIs reales
            ViewBag.MesasOcupadas = todasMesas.Count(m => m.EstadoMesa == "ocupada");
            ViewBag.EnPreparacion = pedidos.Count(p => p.EstadoPedido == "en_proceso" || p.EstadoPedido == "abierto");
            ViewBag.Listos = pedidos.Count(p => p.EstadoPedido == "listo");
            ViewBag.Entregados = pedidos.Count(p => p.EstadoPedido == "entregado");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearOrden(int idMesa, int? idEmpleado, byte comensales, int idProducto, decimal cantidad, string tipoServicio, string observaciones)
        {
            int uid = Session["UsuarioId"] as int? ?? 0;
            if (uid == 0)
                return RedirectToAction("Index", "Login");

            var empleado = db.Empleados.Include(e => e.Usuario.Rol).FirstOrDefault(e =>
                e.IdEmpleado == idEmpleado && e.Estado && e.Usuario.Estado &&
                e.Usuario.Rol.Estado && e.Usuario.Rol.NombreRol == "Mesero");
            if (empleado == null)
            {
                TempData["Error"] = "Debe seleccionar un mesero activo autorizado.";
                return RedirectToAction("Index");
            }

            if (idMesa <= 0 || comensales <= 0 || idProducto <= 0 || cantidad <= 0 || cantidad != decimal.Truncate(cantidad))
            {
                TempData["Error"] = "Debe completar todos los campos obligatorios.";
                return RedirectToAction("Index");
            }

            var mesa = db.Mesas.FirstOrDefault(m => m.IdMesa == idMesa && m.Estado);
            if (mesa == null || mesa.EstadoMesa == "inactiva")
            {
                TempData["Error"] = "La mesa seleccionada no está disponible.";
                return RedirectToAction("Index");
            }

            if (mesa.IdEmpleadoAsignado.HasValue && mesa.IdEmpleadoAsignado != empleado.IdEmpleado)
                throw new ArgumentException("Seleccione el mesero asignado a la mesa o cambie primero la asignación.");
            if (string.Equals(Session["RolNombre"] as string, "Mesero", StringComparison.OrdinalIgnoreCase) && empleado.IdUsuario != uid)
                throw new ArgumentException("Solo puede registrar pedidos para sus propias mesas.");
            if (cantidad > int.MaxValue || cantidad != decimal.Truncate(cantidad))
                throw new ArgumentException("La cantidad de productos debe ser un número entero.");
            if (comensales > mesa.Capacidad)
                throw new ArgumentException("La cantidad de comensales supera la capacidad de la mesa.");

            var producto = db.Productos.FirstOrDefault(p => p.IdProducto == idProducto && p.Estado == true && p.Disponible == true);
            if (producto == null)
            {
                TempData["Error"] = "El producto seleccionado no existe o no está disponible.";
                return RedirectToAction("Index");
            }

            // Solo bloquear si hay una orden ACTIVA (no finalizada ni cancelada) en la misma mesa
            var pedidoMismaMesa = db.Pedidos.FirstOrDefault(p => p.IdMesa == idMesa && p.Estado &&
                p.EstadoPedido != "cancelado" && p.EstadoPedido != "finalizado");
            if (pedidoMismaMesa != null)
            {
                TempData["Error"] = $"La mesa {mesa.NumeroMesa} ya tiene una orden activa (#{pedidoMismaMesa.IdPedido}). Puede agregarle productos abajo.";
                return RedirectToAction("Index");
            }

            if (tipoServicio == "Salón" || tipoServicio == "Salon" || string.IsNullOrWhiteSpace(tipoServicio))
                tipoServicio = "mesa";
            else if (tipoServicio == "Para llevar")
                tipoServicio = "para_llevar";
            else if (tipoServicio == "Delivery")
                tipoServicio = "delivery";
            if (!new[] { "mesa", "para_llevar", "delivery" }.Contains(tipoServicio))
                throw new ArgumentException("Tipo de servicio inválido.");

            string origenMesa = tipoServicio;

            var pedido = new Pedido
            {
                IdMesa = idMesa,
                IdEmpleado = empleado.IdEmpleado,
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
                Cantidad = checked((int)cantidad),
                PrecioUnitario = producto.PrecioVenta,
                ObservacionesItem = observaciones,
                EstadoItem = "pendiente",
                Estado = true
            };

            db.DetallePedidos.Add(detalle);

            // D-08: Descontar insumos de la receta asociada al producto cuando se confirma/crea la orden
            string resumenReceta = InventarioService.DescontarStockPorReceta(db, idProducto, cantidad, uid, $"Pedido #{pedido.IdPedido}");

            db.BitacoraPedidos.Add(new BitacoraPedido
            {
                IdUsuario = uid,
                IdPedido = pedido.IdPedido,
                Accion = "CREACION",
                EstadoNuevo = "abierto",
                Detalle = $"Pedido creado. Mesa {mesa.NumeroMesa}, {cantidad}x {producto.NombreProducto}",
                FechaHora = DateTime.Now
            });

            mesa.EstadoMesa = "ocupada";
            mesa.IdEmpleadoAsignado = empleado.IdEmpleado;

            // Usar Accion = "UPDATE" para cumplir con CK_BitRR_accion en Bitacora_RRHH
            db.BitacoraRRHH.Add(new BitacoraRRHH
            {
                IdUsuario = uid,
                TablaAfectada = "Pedidos",
                IdRegistroAfectado = pedido.IdPedido,
                Accion = "UPDATE",
                ValorNuevo = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    idMesa,
                    idEmpleado = empleado.IdEmpleado,
                    origenMesa
                }),
                Detalle = $"Mesa {mesa.NumeroMesa} asignada al pedido #{pedido.IdPedido} (mesero: {empleado.Nombre} {empleado.Apellidos})",
                FechaHora = DateTime.Now
            });

            db.SaveChanges();

            string msgExtra = !string.IsNullOrEmpty(resumenReceta)
                ? $" Insumos descontados de inventario: {resumenReceta}."
                : "";
            TempData["Mensaje"] = $"Orden #{pedido.IdPedido} creada correctamente para {mesa.NumeroMesa} (Mesero: {empleado.Nombre} {empleado.Apellidos}). Ya aparece en cocina.{msgExtra}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarProducto(int idPedido, int idProducto, decimal cantidad, string observacionesItem)
        {
            if (idPedido <= 0 || idProducto <= 0 || cantidad <= 0 || cantidad > int.MaxValue || cantidad != decimal.Truncate(cantidad))
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

            if (pedido.EstadoPedido != "abierto")
            {
                TempData["Error"] = "No se puede modificar: orden en preparación o no disponible para modificación.";
                return RedirectToAction("Index");
            }

            db.DetallePedidos.Add(new DetallePedido
            {
                IdPedido = idPedido,
                IdProducto = idProducto,
                Cantidad = checked((int)cantidad),
                PrecioUnitario = producto.PrecioVenta,
                ObservacionesItem = observacionesItem,
                EstadoItem = "pendiente",
                Estado = true
            });

            int uid = Session["UsuarioId"] as int? ?? 1;

            // D-08: Descontar insumos de la receta asociada al producto
            string resumenReceta = InventarioService.DescontarStockPorReceta(db, idProducto, cantidad, uid, $"Pedido #{idPedido}");

            db.BitacoraPedidos.Add(new BitacoraPedido
            {
                IdUsuario = uid,
                IdPedido = idPedido,
                Accion = "MODIFICACION",
                Detalle = "Producto agregado: " + cantidad + "x " + producto.NombreProducto,
                FechaHora = DateTime.Now
            });

            db.SaveChanges();

            string msgExtra = !string.IsNullOrEmpty(resumenReceta)
                ? $" Insumos descontados: {resumenReceta}."
                : "";
            TempData["Mensaje"] = $"Producto agregado correctamente a la orden #{idPedido}.{msgExtra}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarProducto(int idDetalle, decimal cantidad, string observacionesItem)
        {
            var detalle = db.DetallePedidos.FirstOrDefault(d => d.IdDetalle == idDetalle && d.Estado == true);

            if (detalle == null)
            {
                TempData["Error"] = "El producto de la orden no existe.";
                return RedirectToAction("Index");
            }

            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == detalle.IdPedido && p.Estado == true);

            if (pedido == null)
            {
                TempData["Error"] = "La orden no existe.";
                return RedirectToAction("Index");
            }

            if (pedido.EstadoPedido != "abierto")
            {
                TempData["Error"] = "No se puede modificar: orden en preparación o no disponible para modificación.";
                return RedirectToAction("Index");
            }

            if (cantidad <= 0 || cantidad > int.MaxValue || cantidad != decimal.Truncate(cantidad))
            {
                TempData["Error"] = "La cantidad debe ser mayor a cero.";
                return RedirectToAction("Index");
            }

            int uid = Session["UsuarioId"] as int? ?? 1;
            decimal diferencia = cantidad - detalle.Cantidad;
            if (diferencia > 0)
            {
                InventarioService.DescontarStockPorReceta(db, detalle.IdProducto, diferencia, uid, $"Edición Pedido #{detalle.IdPedido}");
            }
            else if (diferencia < 0)
            {
                InventarioService.RestaurarStockPorReceta(db, detalle.IdProducto, Math.Abs(diferencia), uid, $"Edición Pedido #{detalle.IdPedido}");
            }

            detalle.Cantidad = checked((int)cantidad);
            detalle.ObservacionesItem = observacionesItem;

            db.BitacoraPedidos.Add(new BitacoraPedido
            {
                IdUsuario = uid,
                IdPedido = detalle.IdPedido,
                Accion = "MODIFICACION",
                Detalle = "Producto modificado: ID detalle " + idDetalle + ", cantidad " + cantidad,
                FechaHora = DateTime.Now
            });

            db.SaveChanges();

            TempData["Mensaje"] = "Producto actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EliminarProducto(int idDetalle)
        {
            var detalle = db.DetallePedidos.FirstOrDefault(d => d.IdDetalle == idDetalle && d.Estado == true);

            if (detalle == null)
            {
                TempData["Error"] = "El producto de la orden no existe.";
                return RedirectToAction("Index");
            }

            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == detalle.IdPedido && p.Estado == true);

            if (pedido == null)
            {
                TempData["Error"] = "La orden no existe.";
                return RedirectToAction("Index");
            }

            if (pedido.EstadoPedido != "abierto")
            {
                TempData["Error"] = "No se puede modificar: orden en preparación o no disponible para modificación.";
                return RedirectToAction("Index");
            }

            int uid = Session["UsuarioId"] as int? ?? 1;

            // D-08: Restaurar insumos al inventario al eliminar producto de orden abierta
            InventarioService.RestaurarStockPorReceta(db, detalle.IdProducto, detalle.Cantidad, uid, $"Eliminación item Pedido #{pedido.IdPedido}");

            detalle.Estado = false;

            db.BitacoraPedidos.Add(new BitacoraPedido
            {
                IdUsuario = uid,
                IdPedido = detalle.IdPedido,
                Accion = "MODIFICACION",
                Detalle = "Producto eliminado: ID detalle " + idDetalle,
                FechaHora = DateTime.Now
            });

            db.SaveChanges();

            var quedanProductos = db.DetallePedidos.Any(d => d.IdPedido == pedido.IdPedido && d.Estado == true);

            if (!quedanProductos)
            {
                pedido.EstadoPedido = "cancelado";

                if (pedido.IdMesa.HasValue)
                {
                    var mesa = db.Mesas.FirstOrDefault(m => m.IdMesa == pedido.IdMesa.Value);
                    if (mesa != null)
                    {
                        mesa.EstadoMesa = "disponible";
                        mesa.IdEmpleadoAsignado = null;
                    }
                }

                db.BitacoraPedidos.Add(new BitacoraPedido
                {
                    IdUsuario = uid,
                    IdPedido = pedido.IdPedido,
                    Accion = "CANCELACION",
                    EstadoAnterior = "abierto",
                    EstadoNuevo = "cancelado",
                    Detalle = "Cancelacion automatica: no quedan productos.",
                    FechaHora = DateTime.Now
                });

                db.SaveChanges();

                TempData["Mensaje"] = "Producto eliminado. La orden quedó cancelada porque no tiene productos.";
                return RedirectToAction("Index");
            }

            TempData["Mensaje"] = "Producto eliminado y total actualizado.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelarPedido(int idPedido, string motivoCancelacion)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado == true);

            if (pedido == null)
            {
                TempData["Error"] = "La orden no existe.";
                return RedirectToAction("Index");
            }

            if (pedido.EstadoPedido != "abierto")
            {
                TempData["Error"] = "No se puede cancelar: la orden ya está en preparación o no está disponible para cancelación.";
                return RedirectToAction("Index");
            }

            int uid = Session["UsuarioId"] as int? ?? 1;

            pedido.EstadoPedido = "cancelado";
            pedido.Observaciones = (pedido.Observaciones ?? "") + " | Cancelación: " + (motivoCancelacion ?? "Sin motivo indicado");

            var detalles = db.DetallePedidos.Where(d => d.IdPedido == idPedido && d.Estado == true).ToList();

            foreach (var item in detalles)
            {
                // D-08: Restaurar insumos al inventario al cancelar orden abierta
                InventarioService.RestaurarStockPorReceta(db, item.IdProducto, item.Cantidad, uid, $"Cancelación Pedido #{idPedido}");
                item.EstadoItem = "cancelado";
                item.Estado = false;
            }

            if (pedido.IdMesa.HasValue)
            {
                var mesa = db.Mesas.FirstOrDefault(m => m.IdMesa == pedido.IdMesa.Value);
                if (mesa != null)
                {
                    mesa.EstadoMesa = "disponible";
                        mesa.IdEmpleadoAsignado = null;
                }
            }

            db.BitacoraPedidos.Add(new BitacoraPedido
            {
                IdUsuario = uid,
                IdPedido = idPedido,
                Accion = "CANCELACION",
                EstadoAnterior = "abierto",
                EstadoNuevo = "cancelado",
                Detalle = "Cancelacion: " + (motivoCancelacion ?? "Sin motivo"),
                FechaHora = DateTime.Now
            });

            db.SaveChanges();

            TempData["Mensaje"] = "Orden cancelada correctamente. Insumos restaurados y mesa liberada.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarcarEntregado(int idPedido)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado);
            if (pedido == null || pedido.EstadoPedido != "listo")
            {
                TempData["Error"] = "Solo se pueden entregar pedidos que estén en estado 'listo'.";
                return RedirectToAction("Index");
            }

            int usuarioActual = (int)Session["UsuarioId"];
            if (string.Equals(Session["RolNombre"] as string, "Mesero", StringComparison.OrdinalIgnoreCase) &&
                !db.Empleados.Any(e => e.IdEmpleado == pedido.IdEmpleado && e.IdUsuario == usuarioActual && e.Estado))
                throw new ArgumentException("Solo el mesero asignado puede entregar el pedido.");
            pedido.EstadoPedido = "entregado";
            pedido.FechaHoraEntrega = DateTime.Now;
            var detalles = db.DetallePedidos.Where(d => d.IdPedido == idPedido && d.Estado).ToList();
            foreach (var d in detalles)
            {
                d.EstadoItem = "entregado";
            }

            int uid = Session["UsuarioId"] as int? ?? 1;
            db.BitacoraPedidos.Add(new BitacoraPedido
            {
                IdUsuario = uid,
                IdPedido = idPedido,
                Accion = "ENTREGA",
                EstadoAnterior = "listo",
                EstadoNuevo = "entregado",
                Detalle = "Pedido entregado al cliente en mesa",
                FechaHora = DateTime.Now
            });

            db.SaveChanges();
            TempData["Mensaje"] = $"Pedido #{idPedido} marcado como Entregado. Ya está listo para cobro en Caja.";
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
