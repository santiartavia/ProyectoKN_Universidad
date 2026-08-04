using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Reportes;
using LogicaDeNegocios.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
    public class ReporteriaController : Controller
    {
        private readonly IContabilidadService _contabilidadService;
        private readonly ReporteService _reporteService;

        public ReporteriaController()
        {
            _contabilidadService = new ContabilidadService(new FechasLN());
            _reporteService = new ReporteService();
        }

        public ActionResult Index()
        {
            ViewBag.Reportes = _contabilidadService.ListarReportes();
            CargarListasFiltros();
            return View();
        }

        [HttpPost]
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
                var parametros = Parametros(tipoReporte, fechaInicio, fechaFin, filtros, formatoOk);

                _contabilidadService.GenerarReporte(idAdmin, tipoReporte, formatoOk, parametros);
                RegistrarBitacora(idAdmin, tipoReporte, fechaInicio, fechaFin, formatoOk, "GENERACION", parametros);

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
        public ActionResult DescargarReporte()
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

        public ActionResult BitacoraReportes(int? idUsuario = null, string accion = null, string fechaInicio = null, string fechaFin = null)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin).Date.AddDays(1).AddSeconds(-1);

            var vm = new RestauranteVistas.Models.ViewModels.BitacoraReportesViewModel
            {
                Registros = _contabilidadService.ConsultarBitacoraReportes(idUsuario, accion, fi, ff),
                Acciones = _contabilidadService.ObtenerAccionesBitacoraReportes(),
                FiltroUsuario = idUsuario,
                FiltroAccion = accion,
                FiltroFechaInicio = fechaInicio,
                FiltroFechaFin = fechaFin
            };
            CargarListasFiltros();
            return View(vm);
        }

        [HttpGet]
        public ActionResult ExportarBitacoraReportes(string formato = "csv", int? idUsuario = null, string accion = null, string fechaInicio = null, string fechaFin = null)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin).Date.AddDays(1).AddSeconds(-1);

            var registros = _contabilidadService.ConsultarBitacoraReportes(idUsuario, accion, fi, ff);
            var empleados = ObtenerNombresEmpleados();

            var headers = new[] { "ID", "Usuario", "Acción", "Detalle", "IP", "Dispositivo", "Fecha/Hora" };
            var rows = registros.Select(r => new[]
            {
                r.IdRegistro.ToString(),
                empleados.ContainsKey(r.IdUsuario) ? empleados[r.IdUsuario] : (r.Usuario?.NombreUsuario ?? ""),
                r.Accion ?? "",
                r.Detalle ?? "",
                r.IpOrigen ?? "",
                r.Dispositivo ?? "",
                r.FechaHora.ToString("yyyy-MM-dd HH:mm:ss")
            }).ToList();

            var res = new ReporteResultado
            {
                Titulo = "Bitácora de Reportes",
                TipoReporte = "bitacora",
                Headers = headers,
                Rows = rows,
                MensajeVacio = "No se encontraron registros en la bitácora de reportes."
            };

            var formatoOk = string.IsNullOrWhiteSpace(formato) ? "csv" : formato.ToLower();
            var bytes = ExportadorReportes.Generar(formatoOk, res);
            _contabilidadService.RegistrarBitacoraReporte(idAdmin, "DESCARGA", $"Bitácora de reportes | {formatoOk}", null,
                Request.UserHostAddress, Request.UserAgent);
            return File(bytes, ExportadorReportes.ContentType(formatoOk), $"bitacora_reportes_{DateTime.Now:yyyyMMdd_HHmmss}{ExportadorReportes.Extension(formatoOk)}");
        }

        [HttpGet]
        public ActionResult RegenerarReporte(int idRegistro)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            using (var ctx = new ColibriDbContext())
            {
                var registro = ctx.BitacoraReportes.FirstOrDefault(b => b.IdRegistro == idRegistro);
                if (registro == null)
                {
                    TempData["Error"] = "Registro de bitácora no encontrado.";
                    return RedirectToAction("BitacoraReportes");
                }

                var p = ParsearParametros(registro.ValorNuevo);
                var tipo = p.ContainsKey("tipo") ? p["tipo"] : "ventas";
                var formato = p.ContainsKey("formato") ? p["formato"] : "csv";
                DateTime? fi = null, ff = null;
                if (p.ContainsKey("fi") && !string.IsNullOrWhiteSpace(p["fi"])) fi = DateTime.Parse(p["fi"]);
                if (p.ContainsKey("ff") && !string.IsNullOrWhiteSpace(p["ff"])) ff = DateTime.Parse(p["ff"]).Date.AddDays(1).AddSeconds(-1);

                var filtros = new ReporteFiltros
                {
                    CategoriaProducto = ValorInt(p, "categoriaProducto"),
                    CategoriaInsumo = ValorInt(p, "categoriaInsumo"),
                    MetodoPago = p.ContainsKey("metodo") ? p["metodo"] : null,
                    IdMesero = ValorInt(p, "idMesero"),
                    TopN = ValorInt(p, "topN"),
                    StockBajo = p.ContainsKey("stockBajo") && p["stockBajo"] == "1",
                    TerminoBusqueda = p.ContainsKey("termino") ? p["termino"] : null,
                    CategoriaEgreso = p.ContainsKey("categoriaEgreso") ? p["categoriaEgreso"] : null
                };

                try
                {
                    var resultado = _reporteService.Generar(tipo, fi, ff, filtros);
                    _contabilidadService.GenerarReporte(idAdmin, tipo, formato, registro.ValorNuevo);

                    if (!resultado.TieneDatos)
                    {
                        TempData["ReporteVacio"] = "true";
                        TempData["ReporteMensajeVacio"] = resultado.MensajeVacio;
                        Session.Remove("ReporteResultado");
                    }
                    else
                    {
                        Session["ReporteResultado"] = resultado;
                        Session["ReporteFormato"] = formato;
                        Session["ReporteDetalle"] = $"Reporte {tipo} regenerado | {formato}";
                    }
                    TempData["Mensaje"] = "Reporte regenerado. Puede descargarlo desde el resultado.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error al regenerar reporte: {ex.Message}";
                    return RedirectToAction("BitacoraReportes");
                }
            }
        }

        [HttpGet]
        public ActionResult ExportarHistorialCsv()
        {
            if (Session["UsuarioId"] == null) return RedirectToAction("Index", "Login");
            var registros = _contabilidadService.ListarReportes();
            var empleados = ObtenerNombresEmpleados();
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

        private void CargarListasFiltros()
        {
            using (var ctx = new ColibriDbContext())
            {
                ViewBag.CategoriasProducto = ctx.CategoriasProducto.Where(c => c.Estado).OrderBy(c => c.NombreCategoria).ToList();
                ViewBag.CategoriasInsumo = ctx.CategoriasInsumo.Where(c => c.Estado).OrderBy(c => c.NombreCategoria).ToList();
                ViewBag.Meseros = ctx.Empleados.Where(e => e.Estado)
                    .OrderBy(e => e.Nombre)
                    .ToDictionary(e => e.IdEmpleado, e => $"{e.Nombre} {e.Apellidos}");
                ViewBag.NombreEmpleados = ctx.Empleados.Where(e => e.Estado)
                    .ToDictionary(e => e.IdUsuario, e => $"{e.Nombre} {e.Apellidos}");
                ViewBag.CategoriasEgreso = new[] { "Servicios públicos", "Mantenimiento", "Limpieza", "Oficina", "Transporte", "Otros" };
            }
        }

        private Dictionary<int, string> ObtenerNombresEmpleados()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Empleados.Where(e => e.Estado)
                    .ToDictionary(e => e.IdUsuario, e => $"{e.Nombre} {e.Apellidos}");
            }
        }

        private void RegistrarBitacora(int idUsuario, string tipoReporte, string fechaInicio, string fechaFin, string formato, string accion, string parametros = null)
        {
            try
            {
                var detalle = $"Reporte {tipoReporte} | {FechaDesc(fechaInicio)} al {FechaDesc(fechaFin)} | {formato}";
                _contabilidadService.RegistrarBitacoraReporte(idUsuario, accion, detalle, parametros,
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

        private static Dictionary<string, string> ParsearParametros(string s)
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(s)) return d;
            foreach (var kv in s.Split(';'))
            {
                var idx = kv.IndexOf('=');
                if (idx < 0) continue;
                d[kv.Substring(0, idx).Trim()] = kv.Substring(idx + 1);
            }
            return d;
        }

        private static int? ValorInt(Dictionary<string, string> d, string key)
        {
            int v;
            if (d.ContainsKey(key) && int.TryParse(d[key], out v)) return v;
            return null;
        }

        private static string FechaDesc(string f)
        {
            DateTime dt;
            return DateTime.TryParse(f, out dt) ? dt.ToString("dd/MM/yyyy") : "—";
        }

        private int ObtenerIdAdmin()
        {
            return Session["UsuarioId"] != null ? (int)Session["UsuarioId"] : 0;
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
