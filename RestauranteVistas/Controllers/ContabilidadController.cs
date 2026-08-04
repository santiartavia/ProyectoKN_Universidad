using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Reportes;
using LogicaDeNegocios.Services;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador", "Cajero" })]
    public class ContabilidadController : Controller
    {
        private readonly IContabilidadService _contabilidadService;
        private readonly ReporteService _reporteService;
        private static readonly string[] CategoriasGasto = { "Servicios públicos", "Mantenimiento", "Limpieza", "Oficina", "Transporte", "Otros" };

        public ContabilidadController()
        {
            _contabilidadService = new ContabilidadService(new FechasLN());
            _reporteService = new ReporteService();
        }

        public ActionResult Index()
        {
            var vm = new ContabilidadViewModel { CategoriasGasto = CategoriasGasto };
            CargarListasFiltros();

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
            sb.AppendLine($"Monto inicial: {D(decimal.Parse(p[3]))}");
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
                sb.AppendLine("ID Cierre;Cajero;Fecha;Apertura;Efectivo;SINPE;Tarjeta;Egresos;Esperado;Real;Descuadre");
                sb.AppendLine($"{cierre.IdCierre};{EscapeCsv(cajeroNombre)};{cierre.FechaCierre:yyyy-MM-dd HH:mm};{D(cierre.MontoApertura)};{D(cierre.TotalEfectivo)};{D(cierre.TotalSinpe)};{D(cierre.TotalTarjeta)};{D(cierre.TotalEgresos)};{D(cierre.SaldoEsperado)};{D(cierre.SaldoReal)};{cierre.Descuadre}");
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
                TempData["ValeEgreso"] = $"{egreso.IdEgreso}|{egreso.CategoriaGasto}|{egreso.Descripcion}|{D(egreso.Monto)}|{egreso.FechaHora:yyyy-MM-dd HH:mm:ss}";
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
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
        public ActionResult GenerarReporte(string tipoReporte, string formato, string fechaInicio, string fechaFin,
            int? categoriaProducto, int? categoriaInsumo, string metodoPago, int? idMesero, int? topN,
            bool? stockBajo, string terminoBusqueda, string categoriaEgreso)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            var reportesConFechas = new[] { "ventas", "productos_mas_vendidos", "ingresos_metodo_pago", "desempenio_meseros", "egresos", "cierre_turno" };
            if (reportesConFechas.Contains(tipoReporte) && (string.IsNullOrWhiteSpace(fechaInicio) || string.IsNullOrWhiteSpace(fechaFin)))
            {
                TempData["ReporteError"] = "Debe seleccionar la fecha de inicio y la fecha final para generar este reporte.";
                return RedirectToAction("Index");
            }

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin).Date.AddDays(1).AddSeconds(-1);

            if (fi.HasValue && ff.HasValue && fi > ff)
            {
                TempData["ReporteError"] = "La fecha de inicio no puede ser mayor que la fecha final.";
                return RedirectToAction("Index");
            }

            try
            {
                var filtros = new ReporteFiltros
                {
                    CategoriaProducto = categoriaProducto,
                    CategoriaInsumo = categoriaInsumo,
                    MetodoPago = metodoPago,
                    IdMesero = idMesero,
                    TopN = topN,
                    StockBajo = stockBajo ?? false,
                    TerminoBusqueda = terminoBusqueda,
                    CategoriaEgreso = categoriaEgreso
                };

                var resultado = _reporteService.Generar(tipoReporte, fi, ff, filtros);
                var formatoOk = string.IsNullOrWhiteSpace(formato) ? "csv" : formato.ToLower();

                _contabilidadService.GenerarReporte(idAdmin, tipoReporte, formatoOk, Parametros(tipoReporte, fechaInicio, fechaFin, filtros, formatoOk));
                RegistrarBitacora(idAdmin, tipoReporte, fechaInicio, fechaFin, formatoOk);

                if (!resultado.TieneDatos)
                {
                    TempData["ReporteVacio"] = "true";
                    TempData["ReporteMensajeVacio"] = resultado.MensajeVacio;
                    Session.Remove("ReporteResultado");
                }
                else
                {
                    Session["ReporteResultado"] = resultado;
                    Session["ReporteFormato"] = formatoOk;
                    Session["ReporteDetalle"] = $"Reporte {tipoReporte} | {FechaDesc(fechaInicio)} al {FechaDesc(fechaFin)} | {formatoOk}";
                }

                TempData["Mensaje"] = "Reporte generado correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar reporte: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
        public ActionResult DescargarReporteGenerado()
        {
            var resultado = Session["ReporteResultado"] as ReporteResultado;
            var formato = Session["ReporteFormato"]?.ToString() ?? "csv";
            if (resultado == null)
            {
                TempData["Error"] = "No hay reporte disponible para descargar.";
                return RedirectToAction("Index");
            }

            var bytes = ExportadorReportes.Generar(formato, resultado);
            var nombre = $"reporte_{resultado.TipoReporte}_{DateTime.Now:yyyyMMdd_HHmmss}{ExportadorReportes.Extension(formato)}";

            var idAdmin = ObtenerIdAdmin();
            if (idAdmin != 0)
            {
                var detalle = Session["ReporteDetalle"]?.ToString();
                _contabilidadService.RegistrarBitacoraReporte(idAdmin, "DESCARGA", detalle ?? $"Reporte {resultado.TipoReporte} | {formato}", null,
                    Request.UserHostAddress, Request.UserAgent);
            }
            return File(bytes, ExportadorReportes.ContentType(formato), nombre);
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
            sb.AppendLine("ID Cierre;Tipo;Periodo;Fecha;Ingresos;Egresos;NC;Saldo Final");
            sb.AppendLine($"{cierre.IdCierrePeriodo};{cierre.TipoPeriodo};{(cierre.TipoPeriodo == "mensual" ? $"{cierre.Mes}/" : "")}{cierre.Anio};{cierre.FechaCierre:yyyy-MM-dd HH:mm};{D(cierre.TotalIngresos)};{D(cierre.TotalEgresos)};{D(cierre.TotalNotasCredito)};{D(cierre.SaldoFinal)}");

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
                sb.AppendLine("ID;Usuario;Acción;Tabla;ID Afectado;Valor Anterior;Valor Nuevo;Detalle;Fecha/Hora");
                foreach (var r in registros)
                {
                    var nom = empleados.ContainsKey(r.IdUsuario) ? empleados[r.IdUsuario] : r.Usuario?.NombreUsuario ?? "";
                    sb.AppendLine($"{r.IdRegistro};{EscapeCsv(nom)};{r.Accion};{EscapeCsv(r.TablaAfectada)};{r.IdRegistroAfectado};{EscapeCsv(r.ValorAnterior)};{EscapeCsv(r.ValorNuevo)};{EscapeCsv(r.Detalle)};{r.FechaHora:yyyy-MM-dd HH:mm:ss}");
                }
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                var bom = Encoding.UTF8.GetPreamble();
                var final = new byte[bom.Length + bytes.Length];
                bom.CopyTo(final, 0);
                bytes.CopyTo(final, bom.Length);
                return File(final, "text/csv", $"bitacora_contable_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
        }

        [HttpGet]
        [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
        public ActionResult ExportarHistorialReportesCsv()
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            var registros = _contabilidadService.ListarReportes();
            using (var ctx = new ColibriDbContext())
            {
                var empleados = ctx.Empleados.Where(e => e.Estado)
                    .ToDictionary(e => e.IdUsuario, e => $"{e.Nombre} {e.Apellidos}");
                var sb = new StringBuilder();
                sb.AppendLine("ID;Tipo;Formato;Fecha;Usuario");
                foreach (var r in registros)
                {
                    var nom = empleados.ContainsKey(r.IdUsuario) ? empleados[r.IdUsuario] : "";
                    sb.AppendLine($"{r.IdReporte};{EscapeCsv(r.TipoReporte)};{r.FormatoSalida.ToUpper()};{r.FechaGeneracion:yyyy-MM-dd HH:mm};{EscapeCsv(nom)}");
                }
                var bytes = Encoding.UTF8.GetBytes(sb.ToString());
                var bom = Encoding.UTF8.GetPreamble();
                var final = new byte[bom.Length + bytes.Length];
                bom.CopyTo(final, 0);
                bytes.CopyTo(final, bom.Length);
                return File(final, "text/csv", $"historial_reportes_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
        }

        private int ObtenerIdAdmin()
        {
            return Session["UsuarioId"] != null ? (int)Session["UsuarioId"] : 0;
        }

        private void CargarListasFiltros()
        {
            using (var ctx = new ColibriDbContext())
            {
                ViewBag.CategoriasProducto = ctx.CategoriasProducto.Where(c => c.Estado).OrderBy(c => c.NombreCategoria).ToList();
                ViewBag.CategoriasInsumo = ctx.CategoriasInsumo.Where(c => c.Estado).OrderBy(c => c.NombreCategoria).ToList();
                ViewBag.Meseros = ctx.Empleados.Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .ToDictionary(e => e.IdEmpleado, e => $"{e.Nombre} {e.Apellidos}");
                ViewBag.CategoriasEgreso = new[] { "Servicios públicos", "Mantenimiento", "Limpieza", "Oficina", "Transporte", "Otros" };
            }
        }

        private void RegistrarBitacora(int idUsuario, string tipoReporte, string fechaInicio, string fechaFin, string formato)
        {
            try
            {
                var detalle = $"Reporte {tipoReporte} | {FechaDesc(fechaInicio)} al {FechaDesc(fechaFin)} | {formato}";
                _contabilidadService.RegistrarBitacoraReporte(idUsuario, "GENERACION", detalle, null,
                    Request.UserHostAddress, Request.UserAgent);
            }
            catch
            {
            }
        }

        private string Parametros(string tipo, string fi, string ff, ReporteFiltros f, string formato)
        {
            var partes = new List<string>
            {
                "tipo=" + tipo,
                "formato=" + (string.IsNullOrWhiteSpace(formato) ? "csv" : formato),
                "fi=" + (fi ?? ""),
                "ff=" + (ff ?? "")
            };
            if (f.CategoriaProducto.HasValue) partes.Add("categoriaProducto=" + f.CategoriaProducto.Value);
            if (f.CategoriaInsumo.HasValue) partes.Add("categoriaInsumo=" + f.CategoriaInsumo.Value);
            if (!string.IsNullOrWhiteSpace(f.MetodoPago)) partes.Add("metodo=" + f.MetodoPago);
            if (f.IdMesero.HasValue) partes.Add("idMesero=" + f.IdMesero.Value);
            if (f.TopN.HasValue) partes.Add("topN=" + f.TopN.Value);
            if (f.StockBajo) partes.Add("stockBajo=1");
            if (!string.IsNullOrWhiteSpace(f.TerminoBusqueda)) partes.Add("termino=" + f.TerminoBusqueda);
            if (!string.IsNullOrWhiteSpace(f.CategoriaEgreso)) partes.Add("categoriaEgreso=" + f.CategoriaEgreso);
            return string.Join(";", partes);
        }

        private static string FechaDesc(string f)
        {
            DateTime dt;
            return DateTime.TryParse(f, out dt) ? dt.ToString("dd/MM/yyyy") : "—";
        }

        private string D(decimal v) => v.ToString("0.00", CultureInfo.InvariantCulture);

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(";") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
