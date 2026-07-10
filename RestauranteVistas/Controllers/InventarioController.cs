using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Services;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
    public class InventarioController : Controller
    {
        private readonly IInventarioService _inventarioService;

        public InventarioController()
        {
            var fechas = new FechasLN();
            _inventarioService = new InventarioService(fechas);
        }

        private int? ObtenerIdUsuarioSesion()
        {
            if (Session["UsuarioId"] == null) return null;
            return (int)Session["UsuarioId"];
        }

        public ActionResult Index(string busqueda = null, int? idCategoria = null, int pagina = 1)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            var todos = _inventarioService.ListarInsumos(busqueda, idCategoria, true);
            int totalRegistros = todos.Count;
            int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)InventarioViewModel.TAMANIO_PAGINA);
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            var paginados = todos.Skip((pagina - 1) * InventarioViewModel.TAMANIO_PAGINA)
                                 .Take(InventarioViewModel.TAMANIO_PAGINA).ToList();

            var modelo = new InventarioViewModel
            {
                Insumos = paginados,
                InsumosBajoStock = _inventarioService.ListarInsumosBajoStock(),
                Categorias = _inventarioService.ListarCategorias(),
                Proveedores = _inventarioService.ListarProveedores(),
                Busqueda = busqueda,
                FiltroCategoria = idCategoria,
                InsumosActivos = _inventarioService.ListarInsumos(null, null, true).Count,
                AlertasMinimas = _inventarioService.ListarInsumosBajoStock().Count,
                TotalCategorias = _inventarioService.ListarCategorias().Count,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                UltimoMovimiento = "Catálogo de insumos"
            };

            if (TempData["Mensaje"] != null)
                modelo.Mensaje = TempData["Mensaje"].ToString();
            if (TempData["Error"] != null)
                modelo.Error = TempData["Error"].ToString();

            return View(modelo);
        }

        [HttpPost]
        public ActionResult RegistrarInsumo(string nombreInsumo, int idCategoria, string unidadMedida, decimal stockMinimo, int? idProveedor)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _inventarioService.RegistrarInsumo(nombreInsumo, idCategoria, unidadMedida, stockMinimo, idAdmin.Value, idProveedor);
                TempData["Mensaje"] = "Insumo registrado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ActualizarInsumo(int idInsumo, string nombreInsumo, int idCategoria, string unidadMedida, decimal stockMinimo)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _inventarioService.ActualizarInsumo(idInsumo, nombreInsumo, idCategoria, unidadMedida, stockMinimo, idAdmin.Value);
                TempData["Mensaje"] = "Insumo actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DesactivarInsumo(int idInsumo)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _inventarioService.DesactivarInsumo(idInsumo, idAdmin.Value);
                TempData["Mensaje"] = "Insumo desactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ReactivarInsumo(int idInsumo)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _inventarioService.ReactivarInsumo(idInsumo, idAdmin.Value);
                TempData["Mensaje"] = "Insumo reactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Inactivos");
        }

        public ActionResult Ajustes(int? idInsumo = null)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            var modelo = new InventarioViewModel
            {
                Insumos = _inventarioService.ListarInsumos(null, null, true),
                Categorias = _inventarioService.ListarCategorias(),
                UltimoMovimiento = "Ajuste manual de existencias"
            };

            if (idInsumo.HasValue)
                ViewBag.InsumoSeleccionado = idInsumo.Value;

            if (TempData["Mensaje"] != null)
                modelo.Mensaje = TempData["Mensaje"].ToString();
            if (TempData["Error"] != null)
                modelo.Error = TempData["Error"].ToString();

            return View(modelo);
        }

        [HttpPost]
        public ActionResult AjustarStock(int idInsumo, decimal cantidad, string tipoAjuste, string motivo)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _inventarioService.AjustarStock(idInsumo, cantidad, tipoAjuste, motivo, idAdmin.Value);

                var insumo = _inventarioService.ObtenerInsumo(idInsumo);
                if (insumo != null && insumo.StockActual < insumo.StockMinimo)
                {
                    TempData["AlertaStock"] = $"El stock de \"{insumo.NombreInsumo}\" quedó en {insumo.StockActual:F2} {insumo.UnidadMedida}, por debajo del mínimo de {insumo.StockMinimo:F2} {insumo.UnidadMedida}.";
                    TempData["Mensaje"] = "Ajuste de existencias procesado.";
                }
                else
                {
                    TempData["Mensaje"] = "Ajuste de existencias procesado correctamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Ajustes", new { idInsumo });
        }

        public ActionResult Bitacora(int? idUsuario = null, string accion = null, string fechaInicio = null, string fechaFin = null)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin).Date.AddDays(1).AddSeconds(-1);

            var registros = _inventarioService.ConsultarBitacora(idUsuario, accion, fi, ff);

            var modelo = new InventarioViewModel
            {
                Bitacora = registros,
                FiltroUsuario = idUsuario,
                FiltroAccion = accion,
                FiltroFechaInicio = fechaInicio,
                FiltroFechaFin = fechaFin,
                AccionesBitacora = _inventarioService.ObtenerAccionesBitacora(),
                UltimoMovimiento = "Bitácora de inventario"
            };

            return View(modelo);
        }

        [HttpGet]
        public ActionResult ExportarBitacoraExcel(int? idUsuario = null, string accion = null, string fechaInicio = null, string fechaFin = null)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin).Date.AddDays(1).AddSeconds(-1);

            var registros = _inventarioService.ConsultarBitacora(idUsuario, accion, fi, ff);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ID,Usuario,Acción,Valor Anterior,Valor Nuevo,Detalle,Fecha/Hora");
            foreach (var r in registros)
            {
                sb.AppendLine($"{r.IdRegistro},{EscapeCsv(r.Usuario?.NombreUsuario)},{r.Accion},{EscapeCsv(r.ValorAnterior)},{EscapeCsv(r.ValorNuevo)},{EscapeCsv(r.Detalle)},{r.FechaHora:yyyy-MM-dd HH:mm:ss}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var preamble = System.Text.Encoding.UTF8.GetPreamble();
            var withBom = new byte[preamble.Length + bytes.Length];
            preamble.CopyTo(withBom, 0);
            bytes.CopyTo(withBom, preamble.Length);

            return File(withBom, "text/csv", $"bitacora_inventario_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        public ActionResult Inactivos(string busqueda = null)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            var inactivos = _inventarioService.ListarInsumos(busqueda, null, false)
                .Where(i => !i.Estado).ToList();

            var modelo = new InventarioViewModel
            {
                Insumos = inactivos,
                Categorias = _inventarioService.ListarCategorias(),
                Busqueda = busqueda,
                UltimoMovimiento = "Insumos inactivos"
            };

            if (TempData["Mensaje"] != null)
                modelo.Mensaje = TempData["Mensaje"].ToString();
            if (TempData["Error"] != null)
                modelo.Error = TempData["Error"].ToString();

            return View(modelo);
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
