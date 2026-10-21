using AccesoADatos;
using Abstracciones.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Cajero", "Administrador" })]
    public class CajeroController : OperacionController
    {


        public ActionResult Index(int? idPedidoDividir = null)
        {
            // Pedidos entregados listos para cobro
            var entregados = db.Pedidos
                .Where(p => p.Estado == true && p.EstadoPedido == "entregado" && p.TipoServicio != "rapida")
                .OrderByDescending(p => p.FechaHoraEntrega ?? p.FechaHora)
                .ToList();
            ViewBag.PedidosEntregados = entregados;

            // Pedidos activos aún en salón/cocina (por si el cajero consulta cuentas abiertas o desea habilitarlos)
            var enCurso = db.Pedidos
                .Where(p => p.Estado == true &&
                            (p.EstadoPedido == "abierto" || p.EstadoPedido == "en_proceso" || p.EstadoPedido == "listo") &&
                            p.TipoServicio != "rapida")
                .OrderByDescending(p => p.FechaHora)
                .ToList();
            ViewBag.PedidosEnCurso = enCurso;

            // Todos los pedidos activos susceptibles de cobro o división de cuenta
            var pedidosActivos = entregados.Concat(enCurso).OrderByDescending(p => p.FechaHora).ToList();
            ViewBag.PedidosActivos = pedidosActivos;

            ViewBag.PedidosFinalizados = db.Pedidos
                .Where(p => p.Estado == true && p.EstadoPedido == "finalizado")
                .OrderByDescending(p => p.FechaHoraFinalizacion ?? p.FechaHora)
                .ToList();

            ViewBag.Detalles = db.DetallePedidos.Where(d => d.Estado == true).ToList();
            ViewBag.Productos = db.Productos.Where(p => p.Estado == true).ToList();
            ViewBag.Mesas = db.Mesas.Where(m => m.Estado == true).ToList();
            ViewBag.Ventas = db.Ventas.Include(v => v.Apertura).Where(v => v.Estado == true).ToList();
            ViewBag.SubcuentasPedido = db.SubcuentasPedido.Where(s => s.Estado == true).ToList();
            ViewBag.DetallesVenta = db.DetallesVenta.Where(d => d.Estado == true).ToList();

            ViewBag.IdPedidoDividir = idPedidoDividir ?? (pedidosActivos.FirstOrDefault()?.IdPedido ?? 0);

            return View();
        }

        /// <summary>
        /// Permite habilitar directamente desde Caja una orden activa a estado 'entregado' para proceder con su cobro.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HabilitarParaCobro(int idPedido)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado);
            if (pedido == null)
            {
                TempData["Error"] = "El pedido no existe.";
                return RedirectToAction("Index");
            }

            if (pedido.EstadoPedido != "listo")
            {
                TempData["Error"] = "Solo se pueden entregar pedidos listos para servir.";
                return RedirectToAction("Index");
            }
            string estadoAnterior = pedido.EstadoPedido;
            pedido.EstadoPedido = "entregado";
            pedido.FechaHoraEntrega = DateTime.Now;

            var detalles = db.DetallePedidos.Where(d => d.IdPedido == idPedido && d.Estado).ToList();
            foreach (var d in detalles)
            {
                d.EstadoItem = "entregado";
            }

            db.HistorialEstadosPedido.Add(new HistorialEstadoPedido
            {
                IdPedido = idPedido,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = "entregado",
                UsuarioResponsable = Session["UsuarioNombre"]?.ToString() ?? "Cajero",
                FechaHoraCambio = DateTime.Now,
                Detalle = "Orden habilitada en Caja como entregada para cobro.",
                Estado = true
            });

            int uid = ObtenerIdUsuarioValido();
            if (uid > 0)
            {
                db.BitacoraPedidos.Add(new BitacoraPedido
                {
                    IdUsuario = uid,
                    IdPedido = idPedido,
                    Accion = "ENTREGA",
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = "entregado",
                    Detalle = "Orden marcada como entregada desde Caja para cobro.",
                    FechaHora = DateTime.Now
                });
            }

            db.SaveChanges();
            TempData["Mensaje"] = $"Pedido #{idPedido} marcado como Entregado. Ya puede registrar su pago o dividir la cuenta.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                TempData["Error"] = "Solo se pueden cobrar pedidos en estado 'Entregado'.";
                return RedirectToAction("Index");
            }

            if (pedido.TipoServicio == "rapida")
            {
                TempData["Error"] = "Este pedido proviene de una venta rápida del PDV. El cobro ya se registra desde el módulo Punto de Venta.";
                return RedirectToAction("Index");
            }

            metodoPago = NormalizarMetodoPago(metodoPago);

            decimal total = CalcularTotalPedido(idPedido);

            if (total <= 0)
            {
                TempData["Error"] = "El pedido no tiene productos para cobrar.";
                return RedirectToAction("Index");
            }

            // C7 Paso 4: Validar monto recibido insuficiente
            if (montoRecibido < total)
            {
                TempData["Error"] = $"Monto recibido insuficiente (₡{montoRecibido:N2}). El monto recibido no puede ser menor al total de la orden (₡{total:N2}).";
                return RedirectToAction("Index");
            }

            var ventaExistente = db.Ventas.FirstOrDefault(v => v.IdPedido == idPedido && v.Estado == true && v.EstadoVenta == "completada");
            var subcuentasAbiertas = db.Ventas.Any(v => v.IdPedido == idPedido && v.Estado == true && v.EstadoVenta == "abierta" && v.IdSubcuenta != null);
            if (subcuentasAbiertas)
            {
                TempData["Error"] = $"La orden #{idPedido} está dividida en subcuentas. Procese el pago individual de cada subcuenta en la sección 'División de Cuentas (Subcuentas)' más abajo.";
                return RedirectToAction("Index");
            }

            if (ventaExistente != null)
            {
                TempData["Error"] = "Este pedido ya tiene su pago completo registrado. Proceda a finalizar la orden.";
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
            var aperturaActiva = ObtenerOCrearAperturaActiva(idEmpleado);

            decimal vuelto = montoRecibido - total;
            var venta = new Venta
            {
                IdPedido = idPedido,
                IdSubcuenta = subcuenta.IdSubcuenta,
                IdEmpleado = idEmpleado,
                IdApertura = aperturaActiva?.IdApertura,
                TipoVenta = "normal",
                TotalCobrado = total,
                MontoRecibido = montoRecibido,
                Vuelto = vuelto,
                MetodoPago = metodoPago,
                EstadoVenta = "completada",
                FechaHora = DateTime.Now,
                Estado = true
            };

            db.Ventas.Add(venta);
            db.SaveChanges();
            foreach (var item in db.DetallePedidos.Where(d => d.IdPedido == idPedido && d.Estado).ToList())
            {
                if (item.Cantidad != decimal.Truncate(item.Cantidad))
                    throw new ArgumentException("La orden contiene una cantidad fraccionaria que requiere revisión antes de facturar.");
                db.DetallesVenta.Add(new DetalleVenta {
                    IdVenta = venta.IdVenta, IdProducto = item.IdProducto,
                    Cantidad = (int)item.Cantidad, PrecioUnitario = item.PrecioUnitario,
                    SubtotalItem = item.Cantidad * item.PrecioUnitario,
                    ObservacionesItem = item.ObservacionesItem, Estado = true
                });
            }

            int uid = ObtenerIdUsuarioValido();
            if (uid > 0)
            {
                db.BitacoraPedidos.Add(new BitacoraPedido
                {
                    IdUsuario = uid,
                    IdPedido = idPedido,
                    Accion = "PAGO",
                    Detalle = $"Pago registrado ({metodoPago}): Total ₡{total:N2}, Recibido ₡{montoRecibido:N2}, Vuelto ₡{vuelto:N2}",
                    FechaHora = DateTime.Now
                });
            }

            db.SaveChanges();

            TempData["Mensaje"] = $"Pago de la orden #{idPedido} registrado correctamente (Total: ₡{total:N2}, Vuelto: ₡{vuelto:N2}). Ahora puede hacer clic en 'Finalizar orden' para liberar la mesa.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinalizarOrden(int idPedido)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado == true);

            if (pedido == null)
            {
                TempData["Error"] = "El pedido no existe.";
                return RedirectToAction("Index");
            }

            // C7 Paso 3: Intentar cerrar la orden sin haber registrado ningún pago -> mostrar mensaje de error
            var ventasPedido = db.Ventas.Where(v => v.IdPedido == idPedido && v.Estado == true).ToList();
            decimal totalPedido = CalcularTotalPedido(idPedido);
            decimal totalPagado = ventasPedido.Where(v => v.EstadoVenta == "completada").Sum(v => (decimal?)v.TotalCobrado) ?? 0m;

            if (!ventasPedido.Any(v => v.EstadoVenta == "completada") || totalPagado < totalPedido)
            {
                TempData["Error"] = $"No se puede finalizar la orden #{idPedido} sin haber registrado el pago completo (Pagado: ₡{totalPagado:N2} de ₡{totalPedido:N2}).";
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
                UsuarioResponsable = Session["UsuarioNombre"]?.ToString() ?? "Cajero",
                FechaHoraCambio = DateTime.Now,
                Detalle = "Cajero finalizó la orden y liberó la mesa.",
                Estado = true
            });

            string nombreMesa = "";
            if (pedido.IdMesa.HasValue)
            {
                var mesa = db.Mesas.FirstOrDefault(m => m.IdMesa == pedido.IdMesa.Value);
                if (mesa != null)
                {
                    mesa.EstadoMesa = "disponible";
                        mesa.IdEmpleadoAsignado = null;
                    nombreMesa = $" y {mesa.NumeroMesa} quedó Disponible";
                }
            }

            db.SaveChanges();

            TempData["Mensaje"] = $"Orden #{idPedido} finalizada exitosamente{nombreMesa}.";
            return RedirectToAction("Index");
        }

        // ============================================================
        // C8 (D-07): División de cuentas (Subcuentas) para órdenes de mesa
        // ============================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearDivisionSubcuentas(int idPedido, int cantidadSubcuentas)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado);
            if (pedido == null)
            {
                TempData["Error"] = "Seleccione una orden válida para dividir la cuenta.";
                return RedirectToAction("Index");
            }

            if (pedido.EstadoPedido == "finalizado" || pedido.EstadoPedido == "cancelado")
            {
                TempData["Error"] = "No se puede dividir una orden que ya fue finalizada o cancelada.";
                return RedirectToAction("Index");
            }

            if (cantidadSubcuentas < 2 || cantidadSubcuentas > 10)
            {
                TempData["Error"] = "Debe indicar al menos 2 subcuentas (máximo 10) para dividir la orden.";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            var detalles = db.DetallePedidos.Where(d => d.IdPedido == idPedido && d.Estado).ToList();
            if (!detalles.Any())
            {
                TempData["Error"] = "La orden seleccionada no tiene productos para dividir.";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            // Verificar si ya tenía subcuentas pagadas
            bool tienePagos = db.Ventas.Any(v => v.IdPedido == idPedido && v.Estado && v.EstadoVenta == "completada");
            if (tienePagos)
            {
                TempData["Error"] = "Esta orden ya tiene pagos registrados y no puede reiniciarse su división.";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            // Limpiar subcuentas abiertas previas si se está reconfigurando antes de pagar
            var ventasPrevias = db.Ventas.Where(v => v.IdPedido == idPedido && v.Estado && v.EstadoVenta == "abierta").ToList();
            foreach (var vp in ventasPrevias)
            {
                var detVp = db.DetallesVenta.Where(dv => dv.IdVenta == vp.IdVenta).ToList();
                db.DetallesVenta.RemoveRange(detVp);
                db.Ventas.Remove(vp);
            }
            var subPrevias = db.SubcuentasPedido.Where(s => s.IdPedido == idPedido && s.Estado).ToList();
            db.SubcuentasPedido.RemoveRange(subPrevias);
            db.SaveChanges();

            int idEmpleado = ObtenerIdEmpleado();
            var apertura = ObtenerOCrearAperturaActiva(idEmpleado);

            for (int i = 1; i <= cantidadSubcuentas; i++)
            {
                var sub = new SubcuentaPedido
                {
                    IdPedido = idPedido,
                    NombreSubcuenta = $"Subcuenta {i}",
                    Estado = true
                };
                db.SubcuentasPedido.Add(sub);
                db.SaveChanges();

                var ventaSub = new Venta
                {
                    IdPedido = idPedido,
                    IdSubcuenta = sub.IdSubcuenta,
                    IdEmpleado = idEmpleado,
                    IdApertura = apertura?.IdApertura,
                    TipoVenta = "normal",
                    TotalCobrado = 0m,
                    MontoRecibido = 0m,
                    Vuelto = 0m,
                    MetodoPago = "efectivo",
                    EstadoVenta = "abierta",
                    FechaHora = DateTime.Now,
                    Estado = true
                };
                db.Ventas.Add(ventaSub);
                db.SaveChanges();
            }

            TempData["Mensaje"] = $"Se habilitaron {cantidadSubcuentas} subcuentas para la orden #{idPedido}. Asigne los productos a cada subcuenta y confirme la división.";
            return RedirectToAction("Index", new { idPedidoDividir = idPedido });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AsignarProductoASubcuentaPedido(int idPedido, int idSubcuenta, int idDetallePedido, int cantidad)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado);
            var subcuenta = db.SubcuentasPedido.FirstOrDefault(s => s.IdSubcuenta == idSubcuenta && s.IdPedido == idPedido && s.Estado);
            var detallePedido = db.DetallePedidos.FirstOrDefault(d => d.IdDetalle == idDetallePedido && d.IdPedido == idPedido && d.Estado);

            if (pedido == null || subcuenta == null || detallePedido == null)
            {
                TempData["Error"] = "Datos de subcuenta o producto inválidos.";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            if (cantidad <= 0)
            {
                TempData["Error"] = "Debe indicar una cantidad mayor a cero para asignar a la subcuenta.";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            var ventaSub = db.Ventas.FirstOrDefault(v => v.IdPedido == idPedido && v.IdSubcuenta == idSubcuenta && v.Estado);
            if (ventaSub == null || ventaSub.EstadoVenta == "completada")
            {
                TempData["Error"] = "La subcuenta seleccionada ya fue pagada o no está activa.";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            // Calcular cuántas unidades de este producto ya están asignadas en todas las subcuentas de este pedido
            var idsVentasPedido = db.Ventas.Where(v => v.IdPedido == idPedido && v.Estado && v.IdSubcuenta != null).Select(v => v.IdVenta).ToList();
            int yaAsignadas = db.DetallesVenta
                .Where(dv => idsVentasPedido.Contains(dv.IdVenta) && dv.IdProducto == detallePedido.IdProducto && dv.ObservacionesItem == ("DET:" + idDetallePedido) && dv.Estado)
                .Sum(dv => (int?)dv.Cantidad) ?? 0;

            int disponibles = (int)detallePedido.Cantidad - yaAsignadas;
            if (cantidad > disponibles)
            {
                TempData["Error"] = $"Solo quedan {disponibles} unidad(es) pendientes por asignar de este producto.";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            var detExistente = db.DetallesVenta.FirstOrDefault(dv =>
                dv.IdVenta == ventaSub.IdVenta && dv.IdProducto == detallePedido.IdProducto &&
                dv.ObservacionesItem == ("DET:" + idDetallePedido) && dv.Estado);

            if (detExistente != null)
            {
                detExistente.Cantidad += cantidad;
                detExistente.SubtotalItem = detExistente.Cantidad * detallePedido.PrecioUnitario;
            }
            else
            {
                db.DetallesVenta.Add(new DetalleVenta
                {
                    IdVenta = ventaSub.IdVenta,
                    IdProducto = detallePedido.IdProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = detallePedido.PrecioUnitario,
                    SubtotalItem = cantidad * detallePedido.PrecioUnitario,
                    ObservacionesItem = "DET:" + idDetallePedido,
                    Estado = true
                });
            }

            db.SaveChanges();

            ventaSub.TotalCobrado = db.DetallesVenta
                .Where(dv => dv.IdVenta == ventaSub.IdVenta && dv.Estado)
                .Sum(dv => (decimal?)dv.SubtotalItem) ?? 0m;
            db.SaveChanges();

            TempData["Mensaje"] = $"Producto asignado a {subcuenta.NombreSubcuenta} (Subtotal actual: ₡{ventaSub.TotalCobrado:N2}).";
            return RedirectToAction("Index", new { idPedidoDividir = idPedido });
        }

        /// <summary>
        /// C8 Paso 3 y Paso 4: Valida que ninguna subcuenta quede vacía sin productos asignados y confirma los subtotales.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarDivisionSubcuentas(int idPedido)
        {
            var subcuentas = db.SubcuentasPedido.Where(s => s.IdPedido == idPedido && s.Estado).ToList();
            if (!subcuentas.Any())
            {
                TempData["Error"] = "Primero debe crear las subcuentas para esta orden.";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            var ventasSub = db.Ventas.Where(v => v.IdPedido == idPedido && v.Estado && v.IdSubcuenta != null).ToList();

            // C8 Paso 3: Intentar dividir la cuenta sin asignar productos a una de las subcuentas
            var subcuentasVacias = new List<string>();
            foreach (var s in subcuentas)
            {
                var vs = ventasSub.FirstOrDefault(v => v.IdSubcuenta == s.IdSubcuenta);
                bool tieneItems = vs != null && db.DetallesVenta.Any(dv => dv.IdVenta == vs.IdVenta && dv.Estado && dv.Cantidad > 0);
                if (!tieneItems)
                {
                    subcuentasVacias.Add(s.NombreSubcuenta);
                }
            }

            if (subcuentasVacias.Any())
            {
                TempData["Error"] = $"Validación: No se permite generar una subcuenta vacía ({string.Join(", ", subcuentasVacias)} no tiene productos asignados). Asigne al menos un producto a cada subcuenta.";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            decimal totalPedido = CalcularTotalPedido(idPedido);
            decimal totalSubcuentas = ventasSub.Sum(v => v.TotalCobrado);

            if (totalSubcuentas != totalPedido)
            {
                TempData["Error"] = $"Aún quedan productos de la orden sin asignar a las subcuentas (Asignado: ₡{totalSubcuentas:N2} de ₡{totalPedido:N2}).";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            TempData["Mensaje"] = $"División de cuenta confirmada para la orden #{idPedido}. Se generaron {subcuentas.Count} subcuentas con sus respectivos subtotales listas para pago individual.";
            return RedirectToAction("Index", new { idPedidoDividir = idPedido });
        }

        /// <summary>
        /// C8 Paso 5: Procesa el pago individual de cada subcuenta y al cancelar la última finaliza la orden principal y libera la mesa.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CobrarSubcuentaPedido(int idPedido, int idSubcuenta, string metodoPago, decimal montoRecibido)
        {
            var subcuenta = db.SubcuentasPedido.FirstOrDefault(s => s.IdSubcuenta == idSubcuenta && s.IdPedido == idPedido && s.Estado);
            var ventaSub = db.Ventas.FirstOrDefault(v => v.IdPedido == idPedido && v.IdSubcuenta == idSubcuenta && v.Estado);

            if (subcuenta == null || ventaSub == null)
            {
                TempData["Error"] = "La subcuenta indicada no existe.";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            if (ventaSub.EstadoVenta == "completada")
            {
                TempData["Error"] = $"La {subcuenta.NombreSubcuenta} ya fue pagada.";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            // Verificar que ninguna subcuenta esté vacía antes de permitir cobrar
            var todasSubcuentas = db.SubcuentasPedido.Where(s => s.IdPedido == idPedido && s.Estado).ToList();
            var todasVentasSub = db.Ventas.Where(v => v.IdPedido == idPedido && v.Estado && v.IdSubcuenta != null).ToList();
            foreach (var s in todasSubcuentas)
            {
                var vs = todasVentasSub.FirstOrDefault(v => v.IdSubcuenta == s.IdSubcuenta);
                if (vs == null || vs.TotalCobrado <= 0)
                {
                    TempData["Error"] = $"No se puede procesar el cobro porque la '{s.NombreSubcuenta}' está vacía sin productos asignados.";
                    return RedirectToAction("Index", new { idPedidoDividir = idPedido });
                }
            }

            decimal totalPedido = CalcularTotalPedido(idPedido);
            decimal sumaSubcuentas = todasVentasSub.Sum(v => v.TotalCobrado);
            if (sumaSubcuentas != totalPedido)
            {
                TempData["Error"] = $"Debe terminar de asignar todos los productos de la orden antes de cobrar las subcuentas (Asignado: ₡{sumaSubcuentas:N2} de ₡{totalPedido:N2}).";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            if (montoRecibido < ventaSub.TotalCobrado)
            {
                TempData["Error"] = $"El monto recibido (₡{montoRecibido:N2}) es insuficiente para cubrir el subtotal de {subcuenta.NombreSubcuenta} (₡{ventaSub.TotalCobrado:N2}).";
                return RedirectToAction("Index", new { idPedidoDividir = idPedido });
            }

            metodoPago = NormalizarMetodoPago(metodoPago);
            ventaSub.MetodoPago = metodoPago;
            ventaSub.MontoRecibido = montoRecibido;
            ventaSub.Vuelto = montoRecibido - ventaSub.TotalCobrado;
            ventaSub.EstadoVenta = "completada";
            ventaSub.FechaHora = DateTime.Now;

            int uid = ObtenerIdUsuarioValido();
            if (uid > 0)
            {
                db.BitacoraPedidos.Add(new BitacoraPedido
                {
                    IdUsuario = uid,
                    IdPedido = idPedido,
                    Accion = "PAGO",
                    Detalle = $"Pago de {subcuenta.NombreSubcuenta} ({metodoPago}): ₡{ventaSub.TotalCobrado:N2}, Vuelto: ₡{ventaSub.Vuelto:N2}",
                    FechaHora = DateTime.Now
                });
            }

            db.SaveChanges();

            // Verificar si todas las subcuentas de este pedido ya fueron pagadas
            bool quedanPendientes = db.Ventas.Any(v => v.IdPedido == idPedido && v.Estado && v.IdSubcuenta != null && v.EstadoVenta != "completada");
            if (!quedanPendientes)
            {
                var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado);
                if (pedido != null)
                {
                    string estadoAnterior = pedido.EstadoPedido;
                    pedido.EstadoPedido = "finalizado";
                    pedido.FechaHoraFinalizacion = DateTime.Now;

                    db.HistorialEstadosPedido.Add(new HistorialEstadoPedido
                    {
                        IdPedido = idPedido,
                        EstadoAnterior = estadoAnterior,
                        EstadoNuevo = "finalizado",
                        UsuarioResponsable = Session["UsuarioNombre"]?.ToString() ?? "Cajero",
                        FechaHoraCambio = DateTime.Now,
                        Detalle = "Todas las subcuentas fueron pagadas; orden finalizada y mesa liberada.",
                        Estado = true
                    });

                    if (pedido.IdMesa.HasValue)
                    {
                        var mesa = db.Mesas.FirstOrDefault(m => m.IdMesa == pedido.IdMesa.Value);
                        if (mesa != null)
                        {
                            mesa.EstadoMesa = "disponible";
                        mesa.IdEmpleadoAsignado = null;
                        }
                    }

                    db.SaveChanges();
                }

                TempData["Mensaje"] = $"¡{subcuenta.NombreSubcuenta} pagada (₡{ventaSub.TotalCobrado:N2})! Todas las subcuentas han sido canceladas: la orden #{idPedido} pasó a estado 'Finalizado' y la mesa quedó 'Disponible'.";
                return RedirectToAction("Index");
            }

            int restantes = db.Ventas.Count(v => v.IdPedido == idPedido && v.Estado && v.IdSubcuenta != null && v.EstadoVenta != "completada");
            TempData["Mensaje"] = $"{subcuenta.NombreSubcuenta} cobrada exitosamente (₡{ventaSub.TotalCobrado:N2}, Vuelto: ₡{ventaSub.Vuelto:N2}). Quedan {restantes} subcuenta(s) pendientes de pago.";
            return RedirectToAction("Index", new { idPedidoDividir = idPedido });
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
            if (string.Equals(metodoPago, "Efectivo", StringComparison.OrdinalIgnoreCase))
                return "efectivo";
            if (string.Equals(metodoPago, "Tarjeta", StringComparison.OrdinalIgnoreCase))
                return "tarjeta";
            if (string.Equals(metodoPago, "SINPE", StringComparison.OrdinalIgnoreCase))
                return "sinpe";
            if (string.Equals(metodoPago, "Mixto", StringComparison.OrdinalIgnoreCase))
                return "mixto";

            throw new ArgumentException("Seleccione un método de pago válido.");
        }

        private AperturaCaja ObtenerOCrearAperturaActiva(int idEmpleado)
        {
            var apertura = db.AperturasCaja.Include(a => a.Caja)
                .Where(a => a.IdCajero == idEmpleado && a.Estado && a.Caja.Estado && a.Caja.EstadoCaja == "abierta")
                .OrderByDescending(a => a.FechaApertura).FirstOrDefault();
            if (apertura == null) throw new ArgumentException("Debe abrir su caja en Contabilidad antes de cobrar.");
            return apertura;
        }

        private int ObtenerIdEmpleado()
        {
            var idUsuario = Session["UsuarioId"] as int?;
            if (idUsuario != null && idUsuario.Value > 0)
            {
                var empleado = db.Empleados.FirstOrDefault(e => e.IdUsuario == idUsuario.Value && e.Estado);
                if (empleado != null) return empleado.IdEmpleado;
            }

            throw new ArgumentException("El usuario no tiene un empleado activo asociado.");
        }

        private int ObtenerIdUsuarioValido()
        {
            int uid = Session["UsuarioId"] as int? ?? 0;
            if (uid > 0) return uid;
            return db.Usuarios.Where(u => u.Estado).Select(u => u.IdUsuario).FirstOrDefault();
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
