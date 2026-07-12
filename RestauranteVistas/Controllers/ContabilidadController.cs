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
            var vm = new ContabilidadViewModel { CategoriasGasto = CategoriasGasto };

            try
            {
                using (var ctx = new ColibriDbContext())
                {
                    var todasLasCajas = ctx.Cajas.Where(c => c.Estado).ToList();
                    vm.CajasCerradas = todasLasCajas.Where(c => c.EstadoCaja == "cerrada").ToList();

                    var idsConCierre = new HashSet<int>(ctx.CierresCaja.Where(c => c.Estado).Select(c => c.IdApertura));
                    var todasAperturas = _contabilidadService.ListarAperturas();

                    vm.AperturasActivas = todasAperturas
                        .Where(a => a.Estado && a.Caja != null && a.Caja.EstadoCaja == "abierta" && !idsConCierre.Contains(a.IdApertura))
                        .ToList();

                    var uidSesion = Session["UsuarioId"];
                    var rolSesion = Session["RolNombre"]?.ToString();
                    int? idEmpleadoCajero = null;
                    if (rolSesion == "Cajero" && uidSesion != null)
                    {
                        int uid = (int)uidSesion;
                        var emp = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == uid && e.Estado);
                        if (emp != null)
                        {
                            idEmpleadoCajero = emp.IdEmpleado;
                            vm.IdCajero = emp.IdEmpleado;
                        }
                    }

                    foreach (var ap in vm.AperturasActivas)
                    {
                        bool enDb = ctx.Cajas.Any(c => c.IdCaja == ap.IdCaja && c.EstadoCaja == "abierta");
                        if (!enDb)
                            ctx.Database.ExecuteSqlCommand("UPDATE Cajas SET estado_caja = 'abierta' WHERE id_caja = {0}", ap.IdCaja);
                    }

                    if (idEmpleadoCajero.HasValue)
                    {
                        vm.AperturasActivas = vm.AperturasActivas
                            .Where(a => a.IdCajero == idEmpleadoCajero.Value)
                            .ToList();
                    }

                    vm.AperturaActiva = vm.AperturasActivas.FirstOrDefault();

                    if (vm.AperturaActiva != null)
                    {
                        int idAp = vm.AperturaActiva.IdApertura;
                        vm.Ventas = ctx.Ventas.Where(v => v.IdApertura == idAp && v.EstadoVenta == "completada" && v.Estado).ToList();
                        vm.NotasCredito = ctx.NotasCredito.Where(n => n.Venta.IdApertura == idAp && n.Estado).ToList();
                        vm.Egresos = _contabilidadService.ListarEgresos(idAp);
                        vm.SaldoDisponible = _contabilidadService.ObtenerSaldoDisponible(idAp);
                        vm.SaldoEsperado = vm.AperturaActiva.MontoInicial
                            + vm.Ventas.Where(v => v.MetodoPago == "efectivo" || v.MetodoPago == "sinpe" || v.MetodoPago == "tarjeta" || v.MetodoPago == "mixto").Sum(v => v.TotalCobrado)
                            - vm.Egresos.Sum(e => e.Monto);
                    }
                    else
                    {
                        vm.Ventas = new List<Venta>();
                        vm.Egresos = new List<EgresoCaja>();
                        vm.NotasCredito = new List<NotaCredito>();
                        vm.SaldoDisponible = 0;
                        vm.SaldoEsperado = 0;
                    }

                    var idsConNC = ctx.NotasCredito.Where(n => n.Estado).Select(n => n.IdVenta);
                    vm.VentasAnulables = ctx.Ventas
                        .Where(v => v.Estado && v.EstadoVenta == "completada" && !idsConNC.Contains(v.IdVenta))
                        .OrderByDescending(v => v.FechaHora)
                        .ToList();

                    vm.VentasCanceladas = ctx.NotasCredito
                        .Include(n => n.Venta)
                        .Include(n => n.Usuario)
                        .Where(n => n.Estado)
                        .OrderByDescending(n => n.FechaHora)
                        .ToList();

                    vm.Cierres = _contabilidadService.ListarCierres();
                    vm.Reportes = _contabilidadService.ListarReportes();
                    vm.CierresPeriodo = _contabilidadService.ListarCierresPeriodo();

                    var empleados = ctx.Empleados.Where(e => e.Estado)
                        .ToDictionary(e => e.IdUsuario, e => $"{e.Nombre} {e.Apellidos}");
                    ViewBag.NombreEmpleados = empleados;
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
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Cajero" })]
        public ActionResult AbrirCaja(int idCaja, int idCajero, decimal montoInicial, string observaciones)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                var apertura = _contabilidadService.AbrirCaja(idCaja, idCajero, montoInicial, observaciones, idAdmin,
                    Request.UserHostAddress, Request.UserAgent);
                TempData["Mensaje"] = $"Caja abierta. Apertura #{apertura.IdApertura}";
                TempData["ResguardoApertura"] = $"{apertura.IdApertura}|{apertura.IdCaja}|{apertura.IdCajero}|{apertura.MontoInicial}|{apertura.FechaApertura:yyyy-MM-dd HH:mm:ss}|{apertura.Observaciones}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Cajero" })]
        public ActionResult DescargarResguardoApertura()
        {
            var data = TempData["ResguardoApertura"]?.ToString();
            if (string.IsNullOrEmpty(data))
            {
                TempData["Error"] = "No hay resguardo disponible.";
                return RedirectToAction("Index");
            }
            var p = data.Split('|');
            var sb = new StringBuilder();
            sb.AppendLine("=== RESGUARDO DE APERTURA DE CAJA ===");
            sb.AppendLine($"Apertura #: {p[0]}");
            sb.AppendLine($"Caja #: {p[1]}");
            sb.AppendLine($"Cajero #: {p[2]}");
            sb.AppendLine($"Monto inicial: {decimal.Parse(p[3]):N2}");
            sb.AppendLine($"Fecha: {p[4]}");
            sb.AppendLine($"Observaciones: {p[5]}");
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/plain", $"resguardo_apertura_{p[0]}.txt");
        }

        [HttpPost]
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
        public ActionResult CerrarCaja(int idApertura, decimal saldoReal)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            int idCajero;
            string cajeroNombre;
            using (var ctx = new ColibriDbContext())
            {
                var ap = ctx.AperturasCaja.Include("Cajero").FirstOrDefault(a => a.IdApertura == idApertura);
                if (ap?.Cajero == null)
                {
                    TempData["Error"] = "No se pudo identificar el cajero de la apertura.";
                    return RedirectToAction("Index");
                }
                idCajero = ap.IdCajero;
                cajeroNombre = ap.Cajero.Nombre + " " + ap.Cajero.Apellidos;
            }

            try
            {
                var cierre = _contabilidadService.CerrarCaja(idApertura, idCajero, saldoReal, idAdmin,
                    Request.UserHostAddress, Request.UserAgent);

                var sb = new StringBuilder();
                sb.AppendLine("ID Cierre,Cajero,Fecha,Apertura,Efectivo,SINPE,Tarjeta,Egresos,Esperado,Real,Descuadre");
                sb.AppendLine($"{cierre.IdCierre},{EscapeCsv(cajeroNombre)},{cierre.FechaCierre:yyyy-MM-dd HH:mm},{cierre.MontoApertura:N2},{cierre.TotalEfectivo:N2},{cierre.TotalSinpe:N2},{cierre.TotalTarjeta:N2},{cierre.TotalEgresos:N2},{cierre.SaldoEsperado:N2},{cierre.SaldoReal:N2},{cierre.Descuadre}");
                TempData["ReporteCierre"] = sb.ToString();
                TempData["ReporteCierreNombre"] = $"cierre_turno_{cierre.IdCierre}.csv";

                if (cierre.Descuadre)
                    TempData["Alerta"] = $"Descuadre: esperado {cierre.SaldoEsperado:C}, real {cierre.SaldoReal:C}.";
                else
                    TempData["Mensaje"] = $"Cierre diario completado con éxito. Cierre #{cierre.IdCierre} - Saldo: {cierre.SaldoReal:C}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult DescargarReporteCierre()
        {
            var csv = TempData.Peek("ReporteCierre")?.ToString();
            var nombre = TempData.Peek("ReporteCierreNombre")?.ToString();
            if (string.IsNullOrEmpty(csv) || string.IsNullOrEmpty(nombre))
            {
                TempData["Error"] = "No hay reporte disponible.";
                return RedirectToAction("Index");
            }
            TempData.Remove("ReporteCierre");
            TempData.Remove("ReporteCierreNombre");
            var bytes = Encoding.UTF8.GetBytes(csv);
            var bom = Encoding.UTF8.GetPreamble();
            var final = new byte[bom.Length + bytes.Length];
            bom.CopyTo(final, 0);
            bytes.CopyTo(final, bom.Length);
            return File(final, "text/csv", nombre);
        }

        [HttpPost]
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
        public ActionResult RegistrarEgreso(int idApertura, string categoriaGasto, string descripcion, decimal monto)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                var egreso = _contabilidadService.RegistrarEgreso(idApertura, idAdmin, categoriaGasto, descripcion, monto,
                    Request.UserHostAddress, Request.UserAgent);
                TempData["Mensaje"] = $"Egreso: {categoriaGasto} - {monto:C}";
                TempData["ValeEgreso"] = $"{egreso.IdEgreso}|{egreso.CategoriaGasto}|{egreso.Descripcion}|{egreso.Monto:N2}|{egreso.FechaHora:yyyy-MM-dd HH:mm:ss}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
        public ActionResult DescargarValeEgreso()
        {
            var data = TempData["ValeEgreso"]?.ToString();
            if (string.IsNullOrEmpty(data))
            {
                TempData["Error"] = "No hay vale disponible.";
                return RedirectToAction("Index");
            }
            var p = data.Split('|');
            var sb = new StringBuilder();
            sb.AppendLine("=== VALE DE EGRESO ===");
            sb.AppendLine($"Egreso #: {p[0]}");
            sb.AppendLine($"Categoría: {p[1]}");
            sb.AppendLine($"Descripción: {p[2]}");
            sb.AppendLine($"Monto: {p[3]}");
            sb.AppendLine($"Fecha: {p[4]}");
            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/plain", $"vale_egreso_{p[0]}.txt");
        }

        [HttpPost]
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
        public ActionResult AnularVenta(int idVenta, string motivo)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                _contabilidadService.AnularVenta(idVenta, idAdmin, motivo,
                    Request.UserHostAddress, Request.UserAgent);
                TempData["Mensaje"] = $"Venta #{idVenta} anulada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
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
                string nombre = $"reporte_{tipoReporte}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                using (var ctx = new ColibriDbContext())
                {
                    switch (tipoReporte)
                    {
                        case "ventas":
                            {
                                var q = ctx.Ventas.Where(v => v.Estado);
                                if (fi.HasValue) q = q.Where(v => v.FechaHora >= fi.Value);
                                if (ff.HasValue) q = q.Where(v => v.FechaHora <= ff.Value);
                                var data = q.OrderByDescending(v => v.FechaHora).ToList();
                                var sb = new StringBuilder();
                                sb.AppendLine("ID Venta,ID Pedido,Método Pago,Total Cobrado,Vuelto,Estado,Fecha/Hora");
                                foreach (var v in data)
                                    sb.AppendLine($"{v.IdVenta},{v.IdPedido},{v.MetodoPago},{v.TotalCobrado:N2},{v.Vuelto:N2},{v.EstadoVenta},{v.FechaHora:yyyy-MM-dd HH:mm:ss}");
                                csv = sb.ToString();
                                break;
                            }
                        case "productos_mas_vendidos":
                            {
                                var q = ctx.DetallePedidos.Where(d => d.Estado);
                                if (fi.HasValue) q = q.Where(d => d.Pedido.FechaHora >= fi.Value);
                                if (ff.HasValue) q = q.Where(d => d.Pedido.FechaHora <= ff.Value);
                                var data = q
                                    .GroupBy(d => new { d.IdProducto, d.Producto.NombreProducto })
                                    .Select(g => new { g.Key.IdProducto, g.Key.NombreProducto, Total = g.Sum(d => d.Cantidad), Ingresos = g.Sum(d => d.Cantidad * d.PrecioUnitario) })
                                    .OrderByDescending(x => x.Total).ToList();
                                var sb = new StringBuilder();
                                sb.AppendLine("ID Producto,Nombre,Unidades Vendidas,Ingresos Totales");
                                foreach (var p in data)
                                    sb.AppendLine($"{p.IdProducto},{EscapeCsv(p.NombreProducto)},{p.Total},{p.Ingresos:N2}");
                                csv = sb.ToString();
                                break;
                            }
                        case "ingresos_metodo_pago":
                            {
                                var q = ctx.Ventas.Where(v => v.Estado && v.EstadoVenta == "completada");
                                if (fi.HasValue) q = q.Where(v => v.FechaHora >= fi.Value);
                                if (ff.HasValue) q = q.Where(v => v.FechaHora <= ff.Value);
                                var data = q
                                    .GroupBy(v => v.MetodoPago)
                                    .Select(g => new { Metodo = g.Key, Total = g.Sum(v => v.TotalCobrado), Cantidad = g.Count() })
                                    .ToList();
                                var sb = new StringBuilder();
                                sb.AppendLine("Método de Pago,Cantidad Ventas,Total Ingresos");
                                foreach (var i in data)
                                    sb.AppendLine($"{i.Metodo},{i.Cantidad},{i.Total:N2}");
                                csv = sb.ToString();
                                break;
                            }
                        case "egresos":
                            {
                                var q = ctx.EgresosCaja.Where(e => e.Estado);
                                if (fi.HasValue) q = q.Where(e => e.FechaHora >= fi.Value);
                                if (ff.HasValue) q = q.Where(e => e.FechaHora <= ff.Value);
                                var data = q.OrderByDescending(e => e.FechaHora).ToList();
                                var sb = new StringBuilder();
                                sb.AppendLine("ID Egreso,Categoría,Descripción,Monto,Fecha/Hora");
                                foreach (var e in data)
                                    sb.AppendLine($"{e.IdEgreso},{EscapeCsv(e.CategoriaGasto)},{EscapeCsv(e.Descripcion)},{e.Monto:N2},{e.FechaHora:yyyy-MM-dd HH:mm:ss}");
                                csv = sb.ToString();
                                break;
                            }
                        case "cierre_turno":
                            {
                                var q = ctx.CierresCaja.Include(c => c.Cajero).Where(c => c.Estado);
                                if (fi.HasValue) q = q.Where(c => c.FechaCierre >= fi.Value);
                                if (ff.HasValue) q = q.Where(c => c.FechaCierre <= ff.Value);
                                var data = q.OrderByDescending(c => c.FechaCierre).ToList();
                                var sb = new StringBuilder();
                                sb.AppendLine("ID Cierre,Cajero,Fecha,Apertura,Efectivo,SINPE,Tarjeta,Egresos,Esperado,Real,Descuadre");
                                foreach (var c in data)
                                    sb.AppendLine($"{c.IdCierre},{EscapeCsv(c.Cajero?.Nombre ?? "")} {EscapeCsv(c.Cajero?.Apellidos ?? "")},{c.FechaCierre:yyyy-MM-dd HH:mm},{c.MontoApertura:N2},{c.TotalEfectivo:N2},{c.TotalSinpe:N2},{c.TotalTarjeta:N2},{c.TotalEgresos:N2},{c.SaldoEsperado:N2},{c.SaldoReal:N2},{c.Descuadre}");
                                csv = sb.ToString();
                                break;
                            }
                        default:
                            throw new ArgumentException($"Tipo de reporte '{tipoReporte}' no válido.");
                    }
                }

                _contabilidadService.GenerarReporte(idAdmin, tipoReporte, formato, null);

                var bytes = Encoding.UTF8.GetBytes(csv);
                var bom = Encoding.UTF8.GetPreamble();
                var final = new byte[bom.Length + bytes.Length];
                bom.CopyTo(final, 0);
                bytes.CopyTo(final, bom.Length);
                return File(final, "text/csv", nombre);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar reporte: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
        public ActionResult GenerarCierrePeriodo(string tipoPeriodo, int? mes, int anio)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                var cierre = _contabilidadService.GenerarCierrePeriodo(idAdmin, tipoPeriodo, mes, anio);
                TempData["Mensaje"] = $"Cierre {tipoPeriodo} {(tipoPeriodo == "mensual" ? $"{mes}/" : "")}{anio} generado. Saldo: {cierre.SaldoFinal:C}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
        public ActionResult ExportarCierrePeriodoCsv(int id)
        {
            var cierres = _contabilidadService.ListarCierresPeriodo();
            var cierre = cierres.FirstOrDefault(c => c.IdCierrePeriodo == id);
            if (cierre == null)
            {
                TempData["Error"] = "Cierre no encontrado.";
                return RedirectToAction("Index");
            }

            var sb = new StringBuilder();
            sb.AppendLine("ID Cierre,Tipo,Periodo,Fecha,Ingresos,Egresos,NC,Saldo Final");
            sb.AppendLine($"{cierre.IdCierrePeriodo},{cierre.TipoPeriodo},{(cierre.TipoPeriodo == "mensual" ? $"{cierre.Mes}/" : "")}{cierre.Anio},{cierre.FechaCierre:yyyy-MM-dd HH:mm},{cierre.TotalIngresos:N2},{cierre.TotalEgresos:N2},{cierre.TotalNotasCredito:N2},{cierre.SaldoFinal:N2}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var bom = Encoding.UTF8.GetPreamble();
            var final = new byte[bom.Length + bytes.Length];
            bom.CopyTo(final, 0);
            bytes.CopyTo(final, bom.Length);
            return File(final, "text/csv", $"cierre_{cierre.TipoPeriodo}_{cierre.Anio}{(cierre.TipoPeriodo == "mensual" ? $"_{cierre.Mes}" : "")}.csv");
        }

        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
        public ActionResult Bitacora(int? idUsuario = null, string accion = null, string fechaInicio = null, string fechaFin = null)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin).Date.AddDays(1).AddSeconds(-1);

            var vm = new ContabilidadViewModel
            {
                BitacoraFinanciera = _contabilidadService.ConsultarBitacoraFinanciera(idUsuario, accion, fi, ff),
                AccionesBitacora = _contabilidadService.ObtenerAccionesBitacoraFinanciera(),
                FiltroUsuario = idUsuario,
                FiltroAccion = accion,
                FiltroFechaInicio = fechaInicio,
                FiltroFechaFin = fechaFin
            };

            using (var ctx = new ColibriDbContext())
            {
                var empleados = ctx.Empleados.Where(e => e.Estado)
                    .ToDictionary(e => e.IdUsuario, e => $"{e.Nombre} {e.Apellidos}");
                ViewBag.NombreEmpleados = empleados;
            }

            return View(vm);
        }

        [HttpGet]
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
        public ActionResult ExportarBitacoraCsv(int? idUsuario = null, string accion = null, string fechaInicio = null, string fechaFin = null)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin).Date.AddDays(1).AddSeconds(-1);

            var registros = _contabilidadService.ConsultarBitacoraFinanciera(idUsuario, accion, fi, ff);

            using (var ctx = new ColibriDbContext())
            {
                var empleados = ctx.Empleados.Where(e => e.Estado)
                    .ToDictionary(e => e.IdUsuario, e => $"{e.Nombre} {e.Apellidos}");
                var sb = new StringBuilder();
                sb.AppendLine("ID,Usuario,Acción,Tabla,ID Afectado,Valor Anterior,Valor Nuevo,Detalle,Fecha/Hora");
                foreach (var r in registros)
                {
                    var nom = empleados.ContainsKey(r.IdUsuario) ? empleados[r.IdUsuario] : r.Usuario?.NombreUsuario ?? "";
                    sb.AppendLine($"{r.IdRegistro},{EscapeCsv(nom)},{r.Accion},{EscapeCsv(r.TablaAfectada)},{r.IdRegistroAfectado},{EscapeCsv(r.ValorAnterior)},{EscapeCsv(r.ValorNuevo)},{EscapeCsv(r.Detalle)},{r.FechaHora:yyyy-MM-dd HH:mm:ss}");
                }
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                var bom = Encoding.UTF8.GetPreamble();
                var final = new byte[bom.Length + bytes.Length];
                bom.CopyTo(final, 0);
                bytes.CopyTo(final, bom.Length);
                return File(final, "text/csv", $"bitacora_contable_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
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
