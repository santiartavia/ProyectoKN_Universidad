using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Services;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador", "Cajero" })]
    public class ContabilidadController : Controller
    {
        private readonly IContabilidadService _contabilidadService;
        private static readonly string[] CategoriasGasto = { "Servicios públicos", "Mantenimiento", "Limpieza", "Oficina", "Transporte", "Otros" };

        public ContabilidadController()
        {
            _contabilidadService = new ContabilidadService(new FechasLN());
        }

        public ActionResult Index()
        {
            var vm = new ContabilidadViewModel
            {
                CategoriasGasto = CategoriasGasto
            };

            try
            {
                using (var ctx = new ColibriDbContext())
                {
                    vm.Cajas = _contabilidadService.ListarCajas();
                    vm.Aperturas = _contabilidadService.ListarAperturas();
                    vm.Cierres = _contabilidadService.ListarCierres();
                    vm.Reportes = _contabilidadService.ListarReportes();

                    var empleados = ctx.Empleados.Where(e => e.Estado)
                        .ToDictionary(e => e.IdUsuario, e => $"{e.Nombre} {e.Apellidos}");
                    ViewBag.NombreEmpleados = empleados;

                    var primeraCaja = vm.Cajas.FirstOrDefault();
                    if (primeraCaja != null)
                        vm.AperturaActiva = _contabilidadService.ObtenerAperturaActiva(primeraCaja.IdCaja);
                    else
                        vm.AperturaActiva = vm.Aperturas.FirstOrDefault(a => a.Caja?.EstadoCaja == "abierta" && a.Estado);

                    if (vm.AperturaActiva != null)
                    {
                        vm.Ventas = ctx.Ventas.Where(v => v.IdApertura == vm.AperturaActiva.IdApertura && v.EstadoVenta == "completada" && v.Estado).ToList();
                        vm.NotasCredito = ctx.NotasCredito.Where(n => n.Venta.IdApertura == vm.AperturaActiva.IdApertura && n.Estado).ToList();
                        vm.Egresos = _contabilidadService.ListarEgresos(vm.AperturaActiva.IdApertura);
                    }
                    else
                    {
                        vm.Ventas = new List<Venta>();
                        vm.Egresos = new List<EgresoCaja>();
                        vm.NotasCredito = new List<NotaCredito>();
                    }

                    var rol = Session["RolNombre"]?.ToString();
                    if (rol == "Cajero" && Session["UsuarioId"] != null)
                    {
                        var idUsuario = (int)Session["UsuarioId"];
                        var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == idUsuario && e.Estado);
                        if (empleado != null)
                            vm.IdCajero = empleado.IdEmpleado;
                    }
                }

                if (TempData["Mensaje"] != null) vm.Mensaje = TempData["Mensaje"].ToString();
                if (TempData["Error"] != null) vm.Error = TempData["Error"].ToString();
                if (TempData["Alerta"] != null) vm.Alerta = TempData["Alerta"].ToString();

                return View(vm);
            }
            catch (Exception ex)
            {
                vm.Error = $"Error al cargar: {ex.Message}";
                return View(vm);
            }
        }

        [HttpPost]
        public ActionResult AbrirCaja(int idCaja, int idCajero, decimal montoInicial, string observaciones)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                var apertura = _contabilidadService.AbrirCaja(idCaja, idCajero, montoInicial, observaciones, idAdmin, Request.UserHostAddress, Request.UserAgent);
                TempData["Mensaje"] = $"Caja abierta exitosamente. Apertura #{apertura.IdApertura}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al abrir caja: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CerrarCaja(int idApertura, decimal saldoReal)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            var idCajero = 0;
            if (Session["RolNombre"]?.ToString() == "Cajero" && Session["UsuarioId"] != null)
            {
                using (var ctx = new ColibriDbContext())
                {
                    var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == (int)Session["UsuarioId"] && e.Estado);
                    if (empleado != null) idCajero = empleado.IdEmpleado;
                }
            }

            if (idCajero == 0)
            {
                TempData["Error"] = "No se pudo identificar al cajero.";
                return RedirectToAction("Index");
            }

            try
            {
                var cierre = _contabilidadService.CerrarCaja(idApertura, idCajero, saldoReal, idAdmin, Request.UserHostAddress, Request.UserAgent);
                if (cierre.Descuadre)
                    TempData["Alerta"] = $"DESCUADRE detectado. Monto esperado: {cierre.SaldoEsperado:C}, monto real: {cierre.SaldoReal:C}.";
                else
                    TempData["Mensaje"] = $"Cierre #{cierre.IdCierre} registrado. Saldo final: {cierre.SaldoReal:C}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cerrar caja: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RegistrarEgreso(int idApertura, string categoriaGasto, string descripcion, decimal monto)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                _contabilidadService.RegistrarEgreso(idApertura, idAdmin, categoriaGasto, descripcion, monto, Request.UserHostAddress, Request.UserAgent);
                TempData["Mensaje"] = $"Egreso registrado: {categoriaGasto} - {monto:C}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al registrar egreso: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AnularVenta(int idVenta, string motivo)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                _contabilidadService.AnularVenta(idVenta, idAdmin, motivo, Request.UserHostAddress, Request.UserAgent);
                TempData["Mensaje"] = $"Venta #{idVenta} anulada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al anular venta: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult GenerarReporte(string tipoReporte, string formato, string fechaInicio, string fechaFin)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                DateTime? fi = null, ff = null;
                if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
                if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin).Date.AddDays(1).AddSeconds(-1);

                string csv;
                string nombreArchivo = $"reporte_{tipoReporte}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                using (var ctx = new ColibriDbContext())
                {
                    switch (tipoReporte)
                    {
                        case "ventas":
                            var qVentas = ctx.Ventas.Where(v => v.Estado);
                            if (fi.HasValue) qVentas = qVentas.Where(v => v.FechaHora >= fi.Value);
                            if (ff.HasValue) qVentas = qVentas.Where(v => v.FechaHora <= ff.Value);
                            var ventas = qVentas.OrderByDescending(v => v.FechaHora).ToList();
                            var sb = new StringBuilder();
                            sb.AppendLine("ID Venta,ID Pedido,Método Pago,Total Cobrado,Vuelto,Estado,Fecha/Hora");
                            foreach (var v in ventas)
                                sb.AppendLine($"{v.IdVenta},{v.IdPedido},{v.MetodoPago},{v.TotalCobrado:N2},{v.Vuelto:N2},{v.EstadoVenta},{v.FechaHora:yyyy-MM-dd HH:mm:ss}");
                            csv = sb.ToString();
                            break;

                        case "productos_mas_vendidos":
                            var qDetalles = ctx.DetallePedidos.Where(d => d.Estado);
                            if (fi.HasValue) qDetalles = qDetalles.Where(d => d.Pedido.FechaHora >= fi.Value);
                            if (ff.HasValue) qDetalles = qDetalles.Where(d => d.Pedido.FechaHora <= ff.Value);
                            var productos = qDetalles
                                .GroupBy(d => new { d.IdProducto, d.Producto.NombreProducto })
                                .Select(g => new { g.Key.IdProducto, g.Key.NombreProducto, Total = g.Sum(d => d.Cantidad), Ingresos = g.Sum(d => d.Cantidad * d.PrecioUnitario) })
                                .OrderByDescending(x => x.Total).ToList();
                            sb = new StringBuilder();
                            sb.AppendLine("ID Producto,Nombre,Unidades Vendidas,Ingresos Totales");
                            foreach (var p in productos)
                                sb.AppendLine($"{p.IdProducto},{EscapeCsv(p.NombreProducto)},{p.Total},{p.Ingresos:N2}");
                            csv = sb.ToString();
                            break;

                        case "ingresos_metodo_pago":
                            var qIngresos = ctx.Ventas.Where(v => v.Estado && v.EstadoVenta == "completada");
                            if (fi.HasValue) qIngresos = qIngresos.Where(v => v.FechaHora >= fi.Value);
                            if (ff.HasValue) qIngresos = qIngresos.Where(v => v.FechaHora <= ff.Value);
                            var ingresos = qIngresos
                                .GroupBy(v => v.MetodoPago)
                                .Select(g => new { Metodo = g.Key, Total = g.Sum(v => v.TotalCobrado), Cantidad = g.Count() })
                                .ToList();
                            sb = new StringBuilder();
                            sb.AppendLine("Método de Pago,Cantidad Ventas,Total Ingresos");
                            foreach (var i in ingresos)
                                sb.AppendLine($"{i.Metodo},{i.Cantidad},{i.Total:N2}");
                            csv = sb.ToString();
                            break;

                        case "egresos":
                            var qEgresos = ctx.EgresosCaja.Where(e => e.Estado);
                            if (fi.HasValue) qEgresos = qEgresos.Where(e => e.FechaHora >= fi.Value);
                            if (ff.HasValue) qEgresos = qEgresos.Where(e => e.FechaHora <= ff.Value);
                            var egresos = qEgresos.OrderByDescending(e => e.FechaHora).ToList();
                            sb = new StringBuilder();
                            sb.AppendLine("ID Egreso,Categoría,Descripción,Monto,Fecha/Hora");
                            foreach (var e in egresos)
                                sb.AppendLine($"{e.IdEgreso},{EscapeCsv(e.CategoriaGasto)},{EscapeCsv(e.Descripcion)},{e.Monto:N2},{e.FechaHora:yyyy-MM-dd HH:mm:ss}");
                            csv = sb.ToString();
                            break;

                        case "cierre_turno":
                            var qCierres = ctx.CierresCaja.Include(c => c.Cajero).Where(c => c.Estado);
                            if (fi.HasValue) qCierres = qCierres.Where(c => c.FechaCierre >= fi.Value);
                            if (ff.HasValue) qCierres = qCierres.Where(c => c.FechaCierre <= ff.Value);
                            var cierres = qCierres.OrderByDescending(c => c.FechaCierre).ToList();
                            sb = new StringBuilder();
                            sb.AppendLine("ID Cierre,Cajero,Fecha,Apertura,Efectivo,SINPE,Tarjeta,Egresos,Esperado,Real,Descuadre");
                            foreach (var c in cierres)
                                sb.AppendLine($"{c.IdCierre},{EscapeCsv(c.Cajero?.Nombre ?? "")} {EscapeCsv(c.Cajero?.Apellidos ?? "")},{c.FechaCierre:yyyy-MM-dd HH:mm},{c.MontoApertura:N2},{c.TotalEfectivo:N2},{c.TotalSinpe:N2},{c.TotalTarjeta:N2},{c.TotalEgresos:N2},{c.SaldoEsperado:N2},{c.SaldoReal:N2},{c.Descuadre}");
                            csv = sb.ToString();
                            break;

                        default:
                            throw new ArgumentException($"Tipo de reporte '{tipoReporte}' no válido.");
                    }
                }

                _contabilidadService.GenerarReporte(idAdmin, tipoReporte, formato, null);

                var bytes = Encoding.UTF8.GetBytes(csv);
                var preamble = Encoding.UTF8.GetPreamble();
                var withBom = new byte[preamble.Length + bytes.Length];
                preamble.CopyTo(withBom, 0);
                bytes.CopyTo(withBom, preamble.Length);

                return File(withBom, "text/csv", nombreArchivo);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar reporte: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        private int ObtenerIdAdmin()
        {
            return Session["UsuarioId"] != null ? (int)Session["UsuarioId"] : 0;
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
