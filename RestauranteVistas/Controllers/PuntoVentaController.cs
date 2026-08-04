using AccesoADatos;
using Abstracciones.Models;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Cajero", "Administrador" })]
    public class PuntoVentaController : Controller
    {
        private ColibriDbContext db = new ColibriDbContext();

        public ActionResult Index()
        {
            return View(CargarViewModel());
        }

        // ============================================================
        // PDV 001 - Venta rápida
        // ============================================================

        [HttpPost]
        public ActionResult AbrirVentaRapida()
        {
            int idEmpleado = ObtenerIdEmpleado();
            if (idEmpleado == 0)
            {
                TempData["Error"] = "No se pudo identificar el cajero de la sesión.";
                return RedirectToAction("Index");
            }

            var apertura = db.AperturasCaja
                .Include(a => a.Caja)
                .Where(a => a.IdCajero == idEmpleado && a.Estado && a.Caja.EstadoCaja == "abierta")
                .OrderByDescending(a => a.FechaApertura)
                .FirstOrDefault();

            if (apertura == null)
            {
                TempData["Error"] = "No hay una apertura de caja activa para este cajero. Debe abrir caja primero antes de registrar ventas rápidas.";
                return RedirectToAction("Index");
            }

            var sesionActiva = db.Ventas.FirstOrDefault(v =>
                v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado &&
                (v.EstadoVenta == "abierta" || v.EstadoVenta == "en_revision" || v.EstadoVenta == "validada"));

            if (sesionActiva != null)
            {
                TempData["Error"] = "Ya existe una sesión de venta rápida activa (venta #" + sesionActiva.IdVenta + ", estado " + sesionActiva.EstadoVenta + "). Cierre o cobre la sesión actual antes de abrir una nueva.";
                return RedirectToAction("Index");
            }

            var venta = new Venta
            {
                IdPedido = null,
                IdSubcuenta = null,
                TipoVenta = "rapida",
                IdEmpleado = idEmpleado,
                IdApertura = apertura.IdApertura,
                TotalCobrado = 0,
                MontoRecibido = 0,
                Vuelto = 0,
                MetodoPago = "efectivo",
                EstadoVenta = "abierta",
                FechaHora = DateTime.Now,
                Estado = true
            };

            db.Ventas.Add(venta);
            db.SaveChanges();
            RegistrarBitacoraPdv("AperturaVenta", apertura.IdCaja, idEmpleado, venta.IdVenta,
                "Apertura de venta rápida #" + venta.IdVenta + " por " + ObtenerNombreCajero());
            db.SaveChanges();

            TempData["Mensaje"] = "Venta rápida #" + venta.IdVenta + " abierta correctamente. Agregue productos para continuar.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AgregarProducto(int idVenta, int idProducto, int cantidad, string observaciones)
        {
            int idEmpleado = ObtenerIdEmpleado();
            var venta = db.Ventas.FirstOrDefault(v =>
                v.IdVenta == idVenta && v.Estado && v.EstadoVenta == "abierta" &&
                v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado);

            if (venta == null)
            {
                TempData["Error"] = "La venta rápida no existe, no está abierta o no pertenece al cajero.";
                return RedirectToAction("Index");
            }

            var producto = db.Productos.FirstOrDefault(p => p.IdProducto == idProducto && p.Estado && p.Disponible);
            if (producto == null)
            {
                TempData["Error"] = "El producto seleccionado no existe o no está disponible.";
                return RedirectToAction("Index");
            }

            if (cantidad <= 0)
            {
                TempData["Error"] = "La cantidad debe ser mayor a cero.";
                return RedirectToAction("Index");
            }

            bool yaTeniaProductos = db.DetallesVenta.Any(d => d.IdVenta == idVenta && d.Estado);

            var detalle = db.DetallesVenta.FirstOrDefault(d => d.IdVenta == idVenta && d.IdProducto == idProducto && d.Estado);
            if (detalle != null)
            {
                detalle.Cantidad += cantidad;
                detalle.SubtotalItem = detalle.Cantidad * detalle.PrecioUnitario;
            }
            else
            {
                db.DetallesVenta.Add(new DetalleVenta
                {
                    IdVenta = idVenta,
                    IdProducto = idProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.PrecioVenta,
                    SubtotalItem = cantidad * producto.PrecioVenta,
                    ObservacionesItem = observaciones,
                    Estado = true
                });
            }

            db.SaveChanges();
            if (yaTeniaProductos)
            {
                RegistrarBitacoraPdv("EdicionVenta", ObtenerIdCaja(venta), idEmpleado, venta.IdVenta,
                    "Producto agregado a la venta rápida #" + venta.IdVenta + ": " + producto.NombreProducto + " x" + cantidad + " por " + ObtenerNombreCajero());
                db.SaveChanges();
            }
            TempData["Mensaje"] = "Producto agregado a la venta rápida.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult QuitarProducto(int idDetalle)
        {
            var detalle = db.DetallesVenta.FirstOrDefault(d => d.IdDetalleVenta == idDetalle && d.Estado);
            if (detalle == null)
            {
                TempData["Error"] = "El detalle de venta no existe.";
                return RedirectToAction("Index");
            }

            int empleadoId = ObtenerIdEmpleado();
            var venta = db.Ventas.FirstOrDefault(v =>
                v.IdVenta == detalle.IdVenta && v.Estado && v.EstadoVenta == "abierta" &&
                v.TipoVenta == "rapida" && v.IdEmpleado == empleadoId);

            if (venta == null)
            {
                TempData["Error"] = "Solo puede quitar productos de una venta rápida abierta.";
                return RedirectToAction("Index");
            }

            db.DetallesVenta.Remove(detalle);
            db.SaveChanges();
            RegistrarBitacoraPdv("EdicionVenta", ObtenerIdCaja(venta), empleadoId, venta.IdVenta,
                "Producto eliminado de la venta rápida #" + venta.IdVenta + " (detalle #" + detalle.IdDetalleVenta + ") por " + ObtenerNombreCajero());
            db.SaveChanges();
            TempData["Mensaje"] = "Producto eliminado de la venta.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CancelarVentaRapida(int idVenta)
        {
            int idEmpleado = ObtenerIdEmpleado();
            var venta = db.Ventas.FirstOrDefault(v =>
                v.IdVenta == idVenta && v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado);

            if (venta == null)
            {
                TempData["Error"] = "La venta rápida no existe o no pertenece al cajero.";
                return RedirectToAction("Index");
            }

            if (venta.EstadoVenta != "abierta")
            {
                TempData["Error"] = "Solo se pueden cancelar ventas rápidas en estado abierta.";
                return RedirectToAction("Index");
            }

            var detalles = db.DetallesVenta.Where(d => d.IdVenta == idVenta).ToList();
            if (detalles.Count > 0)
                db.DetallesVenta.RemoveRange(detalles);

            var apertura = ObtenerApertura(venta.IdEmpleado);
            var envios = db.EnviosPdv.Where(e => e.IdVenta == idVenta).ToList();

            if (venta.IdPedido.HasValue)
            {
                var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == venta.IdPedido.Value);
                if (pedido != null && pedido.EstadoPedido != "cancelado")
                {
                    pedido.EstadoPedido = "cancelado";
                    pedido.Observaciones = (pedido.Observaciones ?? "") + " | Cancelación: venta rápida cancelada.";
                    var itemsPedido = db.DetallePedidos.Where(d => d.IdPedido == pedido.IdPedido && d.Estado).ToList();
                    foreach (var item in itemsPedido)
                    {
                        item.EstadoItem = "cancelado";
                        item.Estado = false;
                    }
                    db.BitacoraPedidos.Add(new BitacoraPedido
                    {
                        IdUsuario = Session["UsuarioId"] as int? ?? 0,
                        IdPedido = pedido.IdPedido,
                        Accion = "CANCELACION",
                        EstadoAnterior = pedido.EstadoPedido,
                        EstadoNuevo = "cancelado",
                        Detalle = "Pedido de cocina cancelado porque la venta rápida fue cancelada.",
                        FechaHora = DateTime.Now
                    });
                }
            }

            RegistrarBitacoraPdv("CancelacionVenta",
                apertura?.IdCaja ?? 0, venta.IdEmpleado, venta.IdVenta,
                "Venta rápida #" + venta.IdVenta + " cancelada antes del cobro.");

            db.Ventas.Remove(venta);
            if (envios.Count > 0)
                db.EnviosPdv.RemoveRange(envios);
            db.SaveChanges();

            TempData["Mensaje"] = "Venta rápida cancelada correctamente sin afectar otros registros.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CobrarVentaRapida(int idVenta, string metodoPago, decimal montoRecibido)
        {
            int idEmpleado = ObtenerIdEmpleado();
            var venta = db.Ventas.FirstOrDefault(v =>
                v.IdVenta == idVenta && v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado);

            if (venta == null)
            {
                TempData["Error"] = "La venta rápida no existe, ya fue cobrada o no pertenece al cajero.";
                return RedirectToAction("Index");
            }

            if (venta.EstadoVenta != "validada")
            {
                TempData["Error"] = "Primero debe validar y confirmar el detalle de la venta antes de cobrarla.";
                return RedirectToAction("Index");
            }

            var detalles = db.DetallesVenta.Where(d => d.IdVenta == idVenta && d.Estado).ToList();
            var errores = ObtenerErroresValidacion(venta, detalles);
            if (errores.Count > 0)
            {
                venta.EstadoVenta = "en_revision";
                RegistrarBitacoraPdv("CobroRapido", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                    "Cobro bloqueado: " + string.Join(" | ", errores));
                db.SaveChanges();
                TempData["Error"] = "No se puede cobrar la venta: " + string.Join(" ", errores);
                return RedirectToAction("Index");
            }

            decimal total = detalles.Sum(d => d.SubtotalItem);

            if (total <= 0)
            {
                TempData["Error"] = "No se puede cobrar una venta vacía. Agregue productos primero.";
                return RedirectToAction("Index");
            }

            if (montoRecibido < total)
            {
                TempData["Error"] = "El monto recibido no puede ser menor al total de la venta.";
                return RedirectToAction("Index");
            }

            venta.TotalCobrado = total;
            venta.MontoRecibido = montoRecibido;
            venta.Vuelto = montoRecibido - total;
            venta.MetodoPago = NormalizarMetodoPago(metodoPago);
            venta.EstadoVenta = "completada";
            venta.FechaHora = DateTime.Now;

            RegistrarBitacoraPdv("CobroRapido", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                "Cobro de venta rápida #" + venta.IdVenta + ": " + venta.MetodoPago + " total " + total.ToString("N2") + ", vuelto " + venta.Vuelto.ToString("N2"));
            db.SaveChanges();

            TempData["Mensaje"] = "Cobro registrado correctamente.";
            return RedirectToAction("Index");
        }

        // ============================================================
        // PDV 003 - Validación y corrección de detalle
        // ============================================================

        [HttpPost]
        public ActionResult CorregirDetalleVenta(int idDetalleVenta, int cantidad, decimal precioUnitario, string observaciones)
        {
            var detalle = db.DetallesVenta.FirstOrDefault(d => d.IdDetalleVenta == idDetalleVenta && d.Estado);
            if (detalle == null)
            {
                TempData["Error"] = "El detalle de venta no existe.";
                return RedirectToAction("Index");
            }

            int idEmpleado = ObtenerIdEmpleado();
            var venta = db.Ventas.FirstOrDefault(v =>
                v.IdVenta == detalle.IdVenta && v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado);
            if (venta == null)
            {
                TempData["Error"] = "No puede corregir este detalle porque la venta no pertenece al cajero.";
                return RedirectToAction("Index");
            }

            if (venta.EstadoVenta == "completada" || venta.EstadoVenta == "anulada")
            {
                TempData["Error"] = "No se puede corregir una venta ya cobrada o anulada.";
                return RedirectToAction("Index");
            }

            if (cantidad <= 0)
            {
                TempData["Error"] = "La cantidad debe ser mayor a cero.";
                return RedirectToAction("Index");
            }

            if (precioUnitario <= 0)
            {
                TempData["Error"] = "El precio unitario debe ser mayor a cero.";
                return RedirectToAction("Index");
            }

            var anterior = new { detalle.Cantidad, detalle.PrecioUnitario, detalle.SubtotalItem, detalle.ObservacionesItem };
            detalle.Cantidad = cantidad;
            detalle.PrecioUnitario = precioUnitario;
            detalle.SubtotalItem = cantidad * precioUnitario;
            detalle.ObservacionesItem = observaciones;

            if (venta.EstadoVenta == "validada")
                venta.EstadoVenta = "abierta";

            RegistrarBitacoraPdv("EdicionVenta", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                "Detalle #" + detalle.IdDetalleVenta + " corregido de " + System.Web.Helpers.Json.Encode(anterior) +
                " a " + System.Web.Helpers.Json.Encode(new { detalle.Cantidad, detalle.PrecioUnitario, detalle.SubtotalItem }));

            db.SaveChanges();
            TempData["Mensaje"] = "Detalle corregido y subtotales recalculados. " + (venta.EstadoVenta == "abierta" ? "La venta debe validarse nuevamente." : "");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ValidarVentaRapida(int idVenta)
        {
            int idEmpleado = ObtenerIdEmpleado();
            var venta = db.Ventas.FirstOrDefault(v =>
                v.IdVenta == idVenta && v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado);
            if (venta == null)
            {
                TempData["Error"] = "La venta rápida no existe o no pertenece al cajero.";
                return RedirectToAction("Index");
            }

            if (venta.EstadoVenta != "abierta" && venta.EstadoVenta != "en_revision")
            {
                TempData["Error"] = "La venta ya fue validada o no se encuentra en revisión.";
                return RedirectToAction("Index");
            }

            var detalles = db.DetallesVenta.Where(d => d.IdVenta == idVenta && d.Estado).ToList();
            var errores = ObtenerErroresValidacion(venta, detalles);
            if (errores.Count > 0)
            {
                venta.EstadoVenta = "en_revision";
                RegistrarBitacoraPdv("EdicionVenta", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                    "Validación rechazada: " + string.Join(" | ", errores));
                db.SaveChanges();
                TempData["Error"] = "No se puede confirmar la venta: " + string.Join(" ", errores);
                return RedirectToAction("Index");
            }

            venta.EstadoVenta = "validada";
            RegistrarBitacoraPdv("AperturaVenta", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                "Venta #" + venta.IdVenta + " validada y lista para cobro.");
            db.SaveChanges();

            var advertencias = ObtenerAdvertenciasValidacion(detalles);
            TempData["Mensaje"] = "Venta validada correctamente y lista para cobro.";
            if (advertencias.Count > 0)
                TempData["Alerta"] = string.Join(" ", advertencias);
            return RedirectToAction("Index");
        }

        // ============================================================
        // PDV 004 - Envío a Cocina/Bar/Caja
        // ============================================================

        [HttpPost]
        public ActionResult EnviarVenta(int idVenta, string destino, string prioridad, string observaciones)
        {
            int idEmpleado = ObtenerIdEmpleado();
            var venta = db.Ventas.FirstOrDefault(v => v.IdVenta == idVenta && v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado);
            if (venta == null)
            {
                TempData["Error"] = "La venta rápida no existe o no pertenece al cajero.";
                return RedirectToAction("Index");
            }

            var detalles = db.DetallesVenta.Where(d => d.IdVenta == idVenta && d.Estado).ToList();
            if (!detalles.Any() || ObtenerErroresValidacion(venta, detalles).Any())
            {
                TempData["Error"] = "No se puede enviar una venta vacía o con datos incompletos.";
                return RedirectToAction("Index");
            }

            destino = destino == "Bar" ? "Bar" : destino == "Caja" ? "Caja" : "Cocina";
            prioridad = prioridad == "Urgente" ? "Urgente" : "Normal";
            int idUsuario = Session["UsuarioId"] as int? ?? 0;

            var enviosPrevios = db.EnviosPdv.Where(e => e.IdVenta == venta.IdVenta).OrderBy(e => e.FechaHoraEnvio).ToList();
            var ultimo = enviosPrevios.OrderByDescending(e => e.FechaHoraEnvio).FirstOrDefault();
            bool esReenvio = enviosPrevios.Any();

            if (esReenvio && ultimo != null && ultimo.EstadoEnvio == "Enviado")
            {
                TempData["Error"] = "La venta #" + venta.IdVenta + " ya fue enviada exitosamente a " + ultimo.Destino + " (confirmación " + ultimo.CodigoConfirmacion + "). No es necesario reenviarla, solo se reenvía cuando existió una falla.";
                return RedirectToAction("Index");
            }

            if (esReenvio)
            {
                foreach (var prev in enviosPrevios.Where(e => e.EstadoEnvio == "Fallido" || e.EstadoEnvio == "Pendiente"))
                {
                    string estadoAnterior = prev.EstadoEnvio;
                    prev.EstadoEnvio = "Reenviado";
                    RegistrarBitacoraPdv("Reenvio", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                        "Envío previo #" + prev.IdEnvio + " de la venta #" + venta.IdVenta + " resuelto mediante reenvío exitoso.");
                }
            }

            string codigo = "PDV-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpperInvariant();
            string estadoFinal = ProcesarTransmision(destino, prioridad);

            if (destino == "Cocina")
                CrearPedidoRapidoDesdeVenta(venta, detalles, idUsuario, observaciones);

            db.EnviosPdv.Add(new EnvioPdv
            {
                IdVenta = venta.IdVenta,
                IdUsuario = idUsuario,
                Destino = destino,
                Prioridad = prioridad,
                EstadoEnvio = estadoFinal,
                CodigoConfirmacion = codigo,
                Observaciones = observaciones,
                MotivoReenvio = esReenvio ? observaciones : null,
                FechaHoraEnvio = DateTime.Now
            });

            RegistrarBitacoraPdv(esReenvio ? "Reenvio" : "Envio", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                "Venta #" + venta.IdVenta + " enviada a " + destino + " (estado " + estadoFinal + ", confirmación " + codigo + ").");
            db.SaveChanges();

            if (estadoFinal == "Fallido")
            {
                TempData["Alerta"] = "No se pudo completar el envío a " + destino + ". La venta queda en estado fallido; corrija y reenvíe nuevamente.";
                return RedirectToAction("Index");
            }

            TempData["Mensaje"] = (esReenvio ? "Reenvío" : "Venta") + " enviado correctamente a " + destino + ". Confirmación: " + codigo;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult MarcarEnvioPendiente(int idEnvio) => ActualizarEstadoEnvio(idEnvio, "Pendiente", "Envio");

        [HttpPost]
        public ActionResult MarcarEnvioFallido(int idEnvio) => ActualizarEstadoEnvio(idEnvio, "Fallido", "Envio");

        private ActionResult ActualizarEstadoEnvio(int idEnvio, string estado, string accion)
        {
            int idEmpleado = ObtenerIdEmpleado();
            var envio = db.EnviosPdv.FirstOrDefault(e => e.IdEnvio == idEnvio);
            if (envio == null)
            {
                TempData["Error"] = "El envío indicado no existe.";
                return RedirectToAction("Index");
            }

            var venta = db.Ventas.FirstOrDefault(v =>
                v.IdVenta == envio.IdVenta && v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado);
            if (venta == null)
            {
                TempData["Error"] = "No puede gestionar el estado de este envío porque la venta no pertenece al cajero.";
                return RedirectToAction("Index");
            }

            string anterior = envio.EstadoEnvio;
            envio.EstadoEnvio = estado;
            RegistrarBitacoraPdv(accion, ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                "Envío #" + envio.IdEnvio + " de la venta #" + venta.IdVenta + " marcado como " + estado + " por " + ObtenerNombreCajero());
            db.SaveChanges();

            TempData["Mensaje"] = "El envío #" + envio.IdEnvio + " fue marcado como " + estado + ".";
            return RedirectToAction("Index");
        }

        // ============================================================
        // PDV 002 - División de cuenta en subcuentas
        // ============================================================

        [HttpPost]
        public ActionResult DividirCuenta(int idVenta, int? cantidadSubcuentas)
        {
            int idEmpleado = ObtenerIdEmpleado();
            var venta = db.Ventas.FirstOrDefault(v =>
                v.IdVenta == idVenta && v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado);
            if (venta == null)
            {
                TempData["Error"] = "La venta rápida no existe o no pertenece al cajero.";
                return RedirectToAction("Index");
            }

            if (venta.EstadoVenta == "completada" || venta.EstadoVenta == "anulada")
            {
                TempData["Error"] = "No se permite dividir una cuenta ya pagada o anulada.";
                return RedirectToAction("Index");
            }

            var subcuentasExistentes = db.SubcuentasVenta.Where(s => s.IdVenta == idVenta && s.Estado).ToList();
            if (subcuentasExistentes.Any())
            {
                TempData["Error"] = "Esta venta ya fue dividida. Gestione las subcuentas existentes.";
                return RedirectToAction("Index");
            }

            var detalles = db.DetallesVenta.Where(d => d.IdVenta == idVenta && d.Estado).ToList();
            if (!detalles.Any())
            {
                TempData["Error"] = "No se puede dividir una venta sin productos.";
                return RedirectToAction("Index");
            }

            int totalProductos = detalles.Sum(d => d.Cantidad);
            if (!cantidadSubcuentas.HasValue || cantidadSubcuentas.Value < 2)
            {
                TempData["Error"] = "Indique al menos 2 subcuentas para dividir la cuenta.";
                return RedirectToAction("Index");
            }

            if (cantidadSubcuentas.Value > totalProductos)
            {
                TempData["Error"] = "La cantidad de subcuentas no puede superar la cantidad de productos de la venta (" + totalProductos + ").";
                return RedirectToAction("Index");
            }

            int numSubcuentas = cantidadSubcuentas.Value;

            for (int i = 0; i < numSubcuentas; i++)
            {
                db.SubcuentasVenta.Add(new SubcuentaVenta
                {
                    IdVenta = idVenta,
                    NombreSubcuenta = "Cliente " + (i + 1),
                    Subtotal = 0,
                    Pagada = false,
                    FechaOperacion = DateTime.Now,
                    Estado = true
                });
            }
            db.SaveChanges();

            RegistrarBitacoraPdv("Division", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                "Venta #" + idVenta + " dividida en " + numSubcuentas + " subcuentas. Asigne los productos que cada cliente pagará.");
            db.SaveChanges();

            TempData["Mensaje"] = "Cuenta dividida en " + numSubcuentas + " subcuentas. Ahora asigne a cada cliente los productos que va a pagar.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AsignarProductoSubcuenta(int idSubcuenta, int idDetalleVenta, int cantidad)
        {
            int idEmpleado = ObtenerIdEmpleado();
            var subcuenta = db.SubcuentasVenta.FirstOrDefault(s => s.IdSubcuenta == idSubcuenta && s.Estado);
            if (subcuenta == null)
            {
                TempData["Error"] = "La subcuenta no existe.";
                return RedirectToAction("Index");
            }

            var venta = db.Ventas.FirstOrDefault(v =>
                v.IdVenta == subcuenta.IdVenta && v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado);
            if (venta == null)
            {
                TempData["Error"] = "La venta asociada no pertenece al cajero.";
                return RedirectToAction("Index");
            }

            if (subcuenta.Pagada)
            {
                TempData["Error"] = "Esta subcuenta ya fue pagada y no puede recibir más productos.";
                return RedirectToAction("Index");
            }

            var detalle = db.DetallesVenta.FirstOrDefault(d => d.IdDetalleVenta == idDetalleVenta && d.IdVenta == subcuenta.IdVenta && d.Estado);
            if (detalle == null)
            {
                TempData["Error"] = "El producto indicado no pertenece a esta venta.";
                return RedirectToAction("Index");
            }

            if (cantidad <= 0)
            {
                TempData["Error"] = "La cantidad a asignar debe ser mayor a cero.";
                return RedirectToAction("Index");
            }

            int asignado = db.SubcuentaDetallesVenta
                .Where(sd => sd.IdDetalleVenta == idDetalleVenta && sd.Estado)
                .Sum(sd => (int?)sd.Cantidad) ?? 0;

            int pendiente = detalle.Cantidad - asignado;
            if (cantidad > pendiente)
            {
                TempData["Error"] = "Solo quedan " + pendiente + " unidades pendientes de este producto. Ya fueron asignadas " + asignado + " a otras subcuentas.";
                return RedirectToAction("Index");
            }

            var existente = db.SubcuentaDetallesVenta
                .FirstOrDefault(sd => sd.IdSubcuenta == idSubcuenta && sd.IdDetalleVenta == idDetalleVenta && sd.Estado);

            if (existente != null)
            {
                existente.Cantidad += cantidad;
                existente.Subtotal = existente.Cantidad * detalle.PrecioUnitario;
            }
            else
            {
                db.SubcuentaDetallesVenta.Add(new SubcuentaDetalleVenta
                {
                    IdSubcuenta = idSubcuenta,
                    IdDetalleVenta = idDetalleVenta,
                    Cantidad = cantidad,
                    Subtotal = cantidad * detalle.PrecioUnitario,
                    FechaOperacion = DateTime.Now,
                    Estado = true
                });
            }

            db.SaveChanges();

            subcuenta.Subtotal = db.SubcuentaDetallesVenta
                .Where(sd => sd.IdSubcuenta == idSubcuenta && sd.Estado)
                .Sum(sd => (decimal?)sd.Subtotal) ?? 0;

            RegistrarBitacoraPdv("Division", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                "Subcuenta #" + subcuenta.IdSubcuenta + " (" + subcuenta.NombreSubcuenta + ") recibe " + cantidad + " unidad(es) del producto del detalle #" + idDetalleVenta);
            db.SaveChanges();

            TempData["Mensaje"] = "Producto asignado a " + subcuenta.NombreSubcuenta + ". Quedan " + pendiente + " unidades pendientes.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult QuitarProductoSubcuenta(int idSubcuentaDetalle)
        {
            int idEmpleado = ObtenerIdEmpleado();
            var item = db.SubcuentaDetallesVenta.FirstOrDefault(sd => sd.IdSubcuentaDetalle == idSubcuentaDetalle && sd.Estado);
            if (item == null)
            {
                TempData["Error"] = "La asignación indicada no existe.";
                return RedirectToAction("Index");
            }

            var subcuenta = db.SubcuentasVenta.FirstOrDefault(s => s.IdSubcuenta == item.IdSubcuenta && s.Estado);
            if (subcuenta == null || subcuenta.Pagada)
            {
                TempData["Error"] = "La subcuenta ya fue pagada o no existe; no se puede quitar el producto.";
                return RedirectToAction("Index");
            }

            var venta = db.Ventas.FirstOrDefault(v =>
                v.IdVenta == subcuenta.IdVenta && v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado);
            if (venta == null)
            {
                TempData["Error"] = "La venta asociada no pertenece al cajero.";
                return RedirectToAction("Index");
            }

            db.SubcuentaDetallesVenta.Remove(item);
            db.SaveChanges();

            subcuenta.Subtotal = db.SubcuentaDetallesVenta
                .Where(sd => sd.IdSubcuenta == subcuenta.IdSubcuenta && sd.Estado)
                .Sum(sd => (decimal?)sd.Subtotal) ?? 0;

            RegistrarBitacoraPdv("Division", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                "Producto retirado de la subcuenta #" + subcuenta.IdSubcuenta + " (" + subcuenta.NombreSubcuenta + ").");
            db.SaveChanges();

            TempData["Mensaje"] = "Producto retirado de la subcuenta.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CobrarSubcuenta(int idSubcuenta, string metodoPago, decimal montoRecibido)
        {
            int idEmpleado = ObtenerIdEmpleado();
            var subcuenta = db.SubcuentasVenta.FirstOrDefault(s => s.IdSubcuenta == idSubcuenta && s.Estado);
            if (subcuenta == null)
            {
                TempData["Error"] = "La subcuenta no existe.";
                return RedirectToAction("Index");
            }

            var venta = db.Ventas.FirstOrDefault(v =>
                v.IdVenta == subcuenta.IdVenta && v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado);
            if (venta == null)
            {
                TempData["Error"] = "La venta asociada no pertenece al cajero.";
                return RedirectToAction("Index");
            }

            if (venta.EstadoVenta == "completada" || venta.EstadoVenta == "anulada")
            {
                TempData["Error"] = "La venta ya fue cerrada o anulada.";
                return RedirectToAction("Index");
            }

            if (subcuenta.Pagada)
            {
                TempData["Error"] = "Esta subcuenta ya fue pagada.";
                return RedirectToAction("Index");
            }

            if (subcuenta.Subtotal <= 0)
            {
                TempData["Error"] = "La subcuenta aún no tiene productos asignados. Asigne los productos que este cliente va a pagar antes de cobrarla.";
                return RedirectToAction("Index");
            }

            if (montoRecibido < subcuenta.Subtotal)
            {
                TempData["Error"] = "El monto recibido no puede ser menor al subtotal de la subcuenta (₡" + subcuenta.Subtotal.ToString("N2") + ").";
                return RedirectToAction("Index");
            }

            subcuenta.Pagada = true;
            RegistrarBitacoraPdv("CobroSubcuenta", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                "Subcuenta #" + subcuenta.IdSubcuenta + " (" + subcuenta.NombreSubcuenta + ") cobrada: " + NormalizarMetodoPago(metodoPago) + " monto " + subcuenta.Subtotal.ToString("N2"));
            db.SaveChanges();

            var detalles = db.DetallesVenta.Where(d => d.IdVenta == venta.IdVenta && d.Estado).ToList();
            int totalProductos = detalles.Sum(d => d.Cantidad);
            var idsDetalle = detalles.Select(d => d.IdDetalleVenta).ToList();
            int asignados = db.SubcuentaDetallesVenta
                .Where(sd => sd.Estado && idsDetalle.Contains(sd.IdDetalleVenta))
                .Sum(sd => (int?)sd.Cantidad) ?? 0;

            var pendientes = db.SubcuentasVenta.Where(s => s.IdVenta == venta.IdVenta && s.Estado && !s.Pagada).ToList();
            if (!pendientes.Any() && totalProductos == asignados)
            {
                venta.EstadoVenta = "completada";
                venta.TotalCobrado = db.SubcuentasVenta.Where(s => s.IdVenta == venta.IdVenta && s.Estado).Sum(s => s.Subtotal);
                venta.MetodoPago = NormalizarMetodoPago(metodoPago);
                venta.MontoRecibido = venta.TotalCobrado;
                venta.Vuelto = 0;
                venta.FechaHora = DateTime.Now;
                RegistrarBitacoraPdv("CobroSubcuenta", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                    "Todas las subcuentas pagadas y todos los productos asignados: la venta principal #" + venta.IdVenta + " se cerró.");
                db.SaveChanges();
                TempData["Mensaje"] = "Subcuenta cobrada. Todas las subcuentas están pagadas y la venta #" + venta.IdVenta + " quedó cerrada.";
                return RedirectToAction("Index");
            }

            if (!pendientes.Any())
            {
                TempData["Alerta"] = "Todas las subcuentas están pagadas, pero aún quedan productos sin asignar a ninguna subcuenta. La venta no se cierra hasta asignar todos los productos.";
            }
            else
            {
                TempData["Mensaje"] = "Subcuenta cobrada correctamente. Faltan " + pendientes.Count + " subcuenta(s) por cobrar.";
            }
            return RedirectToAction("Index");
        }

        // ============================================================
        // Soporte
        // ============================================================

        private PuntoVentaViewModel CargarViewModel()
        {
            int idEmpleado = ObtenerIdEmpleado();

            var ventas = db.Ventas
                .Where(v => v.Estado && v.TipoVenta == "rapida" && v.IdEmpleado == idEmpleado &&
                    (v.EstadoVenta == "abierta" || v.EstadoVenta == "en_revision" || v.EstadoVenta == "validada"))
                .OrderByDescending(v => v.FechaHora)
                .ToList();

            var productoNombres = db.Productos
                .Where(p => p.Estado)
                .ToDictionary(p => p.IdProducto, p => p.NombreProducto);

            var idsVentas = ventas.Select(v => v.IdVenta).ToList();
            var edicionesPorVenta = db.BitacoraPdv
                .Where(b => b.IdVenta != null && idsVentas.Contains(b.IdVenta.Value) && b.AccionOperativa == "EdicionVenta")
                .GroupBy(b => b.IdVenta.Value)
                .Select(g => new { IdVenta = g.Key, Cantidad = g.Count() })
                .ToDictionary(x => x.IdVenta, x => x.Cantidad);

            var vm = new PuntoVentaViewModel
            {
                TieneAperturaActiva = db.AperturasCaja
                    .Include(a => a.Caja)
                    .Any(a => a.IdCajero == idEmpleado && a.Estado && a.Caja.EstadoCaja == "abierta"),
                NombreCajero = ObtenerNombreCajero(),
                Productos = db.Productos.Where(p => p.Estado && p.Disponible).OrderBy(p => p.NombreProducto).ToList(),
                VentasActivas = ventas.Select(v =>
                {
                    var detalles = db.DetallesVenta.Where(d => d.IdVenta == v.IdVenta && d.Estado).ToList();
                    var subcuentas = db.SubcuentasVenta.Where(s => s.IdVenta == v.IdVenta && s.Estado).OrderBy(s => s.IdSubcuenta).ToList();
                    var idsDetalle = detalles.Select(d => d.IdDetalleVenta).ToList();
                    var items = db.SubcuentaDetallesVenta
                        .Where(sd => idsDetalle.Contains(sd.IdDetalleVenta) && sd.Estado)
                        .ToList();
                    var nombresSub = subcuentas.ToDictionary(s => s.IdSubcuenta, s => s.NombreSubcuenta);

                    var pendiente = new Dictionary<int, int>();
                    foreach (var d in detalles)
                    {
                        int asignado = items.Where(i => i.IdDetalleVenta == d.IdDetalleVenta).Sum(i => i.Cantidad);
                        pendiente[d.IdDetalleVenta] = d.Cantidad - asignado;
                    }

                    return new PuntoVentaViewModel.VentaActiva
                    {
                        Venta = v,
                        Detalles = detalles,
                        Total = detalles.Sum(d => d.SubtotalItem),
                        Envios = db.EnviosPdv.Where(e => e.IdVenta == v.IdVenta).OrderByDescending(e => e.FechaHoraEnvio).ToList(),
                        Subcuentas = subcuentas,
                        SubcuentaItems = items.Select(i =>
                        {
                            var det = detalles.FirstOrDefault(d => d.IdDetalleVenta == i.IdDetalleVenta);
                            string nombre = det != null && productoNombres.ContainsKey(det.IdProducto)
                                ? productoNombres[det.IdProducto] : "Producto #" + (det?.IdProducto ?? 0);
                            return new PuntoVentaViewModel.SubcuentaVentaDetalle
                            {
                                IdSubcuenta = i.IdSubcuenta,
                                NombreSubcuenta = nombresSub.ContainsKey(i.IdSubcuenta) ? nombresSub[i.IdSubcuenta] : ("Subcuenta #" + i.IdSubcuenta),
                                Pagada = subcuentas.FirstOrDefault(s => s.IdSubcuenta == i.IdSubcuenta)?.Pagada ?? false,
                                IdSubcuentaDetalle = i.IdSubcuentaDetalle,
                                IdDetalleVenta = i.IdDetalleVenta,
                                NombreProducto = nombre,
                                Cantidad = i.Cantidad,
                                Subtotal = i.Subtotal
                            };
                        }).ToList(),
                        PendientePorDetalle = pendiente,
                        Ediciones = edicionesPorVenta.ContainsKey(v.IdVenta) ? edicionesPorVenta[v.IdVenta] : 0
                    };
                }).Select(va =>
                {
                    va.MuyEditada = va.Ediciones >= 3;
                    return va;
                }).ToList()
            };

            if (TempData["Mensaje"] != null) vm.Mensaje = TempData["Mensaje"] as string;
            if (TempData["Error"] != null) vm.Error = TempData["Error"] as string;
            if (TempData["Alerta"] != null) vm.Alerta = TempData["Alerta"] as string;
            return vm;
        }

        private List<string> ObtenerErroresValidacion(Venta venta, List<DetalleVenta> detalles)
        {
            var errores = new List<string>();

            foreach (var d in detalles)
            {
                var producto = db.Productos.FirstOrDefault(p => p.IdProducto == d.IdProducto);
                string nombre = producto != null ? producto.NombreProducto : "Producto #" + d.IdProducto;

                if (d.Cantidad <= 0)
                    errores.Add("Existen cantidades inválidas en el detalle (" + nombre + ").");

                if (d.PrecioUnitario <= 0)
                    errores.Add("Existen errores en el precio de uno o más productos (" + nombre + ").");

                if (d.SubtotalItem != Math.Round(d.Cantidad * d.PrecioUnitario, 2))
                    errores.Add("El subtotal de " + nombre + " no es consistente.");
            }

            decimal totalReal = detalles.Sum(d => d.SubtotalItem);
            if (venta.TotalCobrado != 0 && totalReal != venta.TotalCobrado)
                errores.Add("El total de la venta no coincide con la suma de los subtotales.");

            return errores;
        }

        private List<string> ObtenerAdvertenciasValidacion(List<DetalleVenta> detalles)
        {
            var advertencias = new List<string>();
            var duplicados = detalles
                .Where(d => d.Estado)
                .GroupBy(d => d.IdProducto)
                .Where(g => g.Count() > 1);

            foreach (var g in duplicados)
            {
                var producto = db.Productos.FirstOrDefault(p => p.IdProducto == g.Key);
                string nombre = producto != null ? producto.NombreProducto : "Producto #" + g.Key;
                advertencias.Add("El producto " + nombre + " aparece repetido en varias líneas; revise si corresponde unificarlas.");
            }

            return advertencias;
        }

        private void RegistrarBitacoraPdv(string accion, int idCaja, int idEmpleado, int? idVenta, string detalle)
        {
            db.BitacoraPdv.Add(new BitacoraPdv
            {
                IdCaja = idCaja,
                IdUsuarioCajero = idEmpleado,
                AccionOperativa = accion,
                IdVenta = idVenta,
                Detalle = detalle,
                FechaHora = DateTime.Now,
                Estado = true
            });
        }

        private AperturaCaja ObtenerApertura(int idEmpleado)
        {
            return db.AperturasCaja
                .Include(a => a.Caja)
                .Where(a => a.IdCajero == idEmpleado && a.Estado)
                .OrderByDescending(a => a.FechaApertura)
                .FirstOrDefault();
        }

        private int ObtenerIdCaja(Venta venta)
        {
            var apertura = ObtenerApertura(venta.IdEmpleado);
            return apertura?.IdCaja ?? 0;
        }

        private string ProcesarTransmision(string destino, string prioridad)
        {
            return "Enviado";
        }

        private void CrearPedidoRapidoDesdeVenta(Venta venta, List<DetalleVenta> detalles, int idUsuario, string observaciones)
        {
            if (venta.IdPedido.HasValue)
                return;

            var pedido = new Pedido
            {
                IdMesa = null,
                IdEmpleado = venta.IdEmpleado,
                TipoServicio = "rapida",
                CantidadComensales = null,
                EstadoPedido = "abierto",
                FechaHora = DateTime.Now,
                Observaciones = string.IsNullOrWhiteSpace(observaciones) ? "Venta rápida" : observaciones,
                Estado = true
            };
            db.Pedidos.Add(pedido);
            db.SaveChanges();

            foreach (var detalle in detalles)
            {
                db.DetallePedidos.Add(new DetallePedido
                {
                    IdPedido = pedido.IdPedido,
                    IdProducto = detalle.IdProducto,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    ObservacionesItem = detalle.ObservacionesItem,
                    EstadoItem = "pendiente",
                    Estado = true
                });
            }

            venta.IdPedido = pedido.IdPedido;

            db.BitacoraPedidos.Add(new BitacoraPedido
            {
                IdUsuario = idUsuario,
                IdPedido = pedido.IdPedido,
                Accion = "CREACION",
                EstadoNuevo = "abierto",
                Detalle = "Pedido rápido creado desde venta #" + venta.IdVenta + " (sin mesa) para la cola de cocina.",
                FechaHora = DateTime.Now
            });

            RegistrarBitacoraPdv("Envio", ObtenerIdCaja(venta), venta.IdEmpleado, venta.IdVenta,
                "Venta #" + venta.IdVenta + " pasó a la cola de cocina como pedido #" + pedido.IdPedido + " (venta rápida, sin mesa).");
        }

        private string NormalizarMetodoPago(string metodoPago)
        {
            if (metodoPago == "Efectivo") return "efectivo";
            if (metodoPago == "Tarjeta") return "tarjeta";
            if (metodoPago == "SINPE") return "sinpe";
            if (metodoPago == "Mixto") return "mixto";
            return "efectivo";
        }

        private int ObtenerIdEmpleado()
        {
            var idUsuario = Session["UsuarioId"] as int?;
            if (idUsuario == null || idUsuario.Value == 0) return 0;

            var empleado = db.Empleados.FirstOrDefault(e => e.IdUsuario == idUsuario.Value && e.Estado);
            return empleado?.IdEmpleado ?? 0;
        }

        private string ObtenerNombreCajero()
        {
            var idUsuario = Session["UsuarioId"] as int?;
            if (idUsuario == null || idUsuario.Value == 0) return "";

            var empleado = db.Empleados.FirstOrDefault(e => e.IdUsuario == idUsuario.Value && e.Estado);
            return empleado == null ? "" : (empleado.Nombre + " " + empleado.Apellidos).Trim();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}