using Abstracciones.Interfaces;
using Abstracciones.Models;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Services;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
    public class ProductosController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly IInventarioService _inventarioService;

        public ProductosController()
        {
            var fechas = new FechasLN();
            _productoService = new ProductoService(fechas);
            _inventarioService = new InventarioService(fechas);
        }

        private int? ObtenerIdUsuarioSesion()
        {
            if (Session["UsuarioId"] == null) return null;
            return (int)Session["UsuarioId"];
        }

        public ActionResult Index(string busqueda = null, int? idCategoria = null, int? idRecetaProducto = null, int pagina = 1)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            var productos = _productoService.ListarProductos(busqueda, idCategoria);
            int totalRegistros = productos.Count;
            int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)ProductosViewModel.TAMANIO_PAGINA);
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            var paginados = productos.Skip((pagina - 1) * ProductosViewModel.TAMANIO_PAGINA)
                                     .Take(ProductosViewModel.TAMANIO_PAGINA).ToList();

            var modelo = new ProductosViewModel
            {
                Productos = paginados,
                CategoriasProducto = _productoService.ListarCategoriasProducto(),
                Insumos = _inventarioService.ListarInsumos(null, null, true),
                Busqueda = busqueda,
                FiltroCategoria = idCategoria,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                TotalProductos = totalRegistros,
                TotalCategorias = _productoService.ListarCategoriasProducto().Count,
            };

            if (idRecetaProducto.HasValue)
            {
                var receta = _productoService.ObtenerRecetaPorProducto(idRecetaProducto.Value);
                modelo.ProductoSeleccionado = _productoService.ObtenerProducto(idRecetaProducto.Value);
                if (receta != null)
                {
                    modelo.RecetaActual = receta;
                    modelo.RecetaInsumos = _productoService.ObtenerRecetaInsumos(receta.IdReceta);
                    ViewBag.MostrarModalReceta = true;
                }
                else
                {
                    ViewBag.MostrarModalReceta = true;
                }
            }

            if (TempData["Mensaje"] != null)
                modelo.Mensaje = TempData["Mensaje"].ToString();
            if (TempData["Error"] != null)
                modelo.Error = TempData["Error"].ToString();

            return View(modelo);
        }

        [HttpPost]
        public ActionResult RegistrarProducto(string nombreProducto, string descripcion, decimal precioVenta, int idCategoriaProd, bool disponible)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _productoService.RegistrarProducto(nombreProducto, descripcion, precioVenta, idCategoriaProd, disponible);
                TempData["Mensaje"] = "Producto registrado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ActualizarProducto(int idProducto, string nombreProducto, string descripcion, decimal precioVenta, int idCategoriaProd, bool disponible)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _productoService.ActualizarProducto(idProducto, nombreProducto, descripcion, precioVenta, idCategoriaProd, disponible);
                TempData["Mensaje"] = "Producto actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DesactivarProducto(int idProducto)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _productoService.DesactivarProducto(idProducto);
                TempData["Mensaje"] = "Producto desactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ReactivarProducto(int idProducto)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _productoService.ReactivarProducto(idProducto);
                TempData["Mensaje"] = "Producto reactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        public ActionResult Receta(int idProducto)
        {
            return RedirectToAction("Index", new { idRecetaProducto = idProducto });
        }

        [HttpPost]
        public ActionResult CrearReceta(int idProducto, int[] idInsumo, decimal[] cantidadUsar)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _productoService.CrearRecetaCompleta(idProducto, idInsumo, cantidadUsar);
                TempData["Mensaje"] = "Receta creada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index", new { idRecetaProducto = idProducto });
        }

        [HttpPost]
        public ActionResult AgregarInsumoReceta(int idReceta, int idProducto, int idInsumo, decimal cantidadUsar)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _productoService.AgregarInsumoReceta(idReceta, idInsumo, cantidadUsar);
                TempData["Mensaje"] = "Insumo agregado a la receta.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index", new { idRecetaProducto = idProducto });
        }

        [HttpPost]
        public ActionResult EliminarInsumoReceta(int idReceta, int idProducto, int idInsumo)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _productoService.EliminarInsumoReceta(idReceta, idInsumo);
                TempData["Mensaje"] = "Insumo eliminado de la receta.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index", new { idRecetaProducto = idProducto });
        }

        [HttpPost]
        public ActionResult AlternarEstadoReceta(int idReceta, int idProducto)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _productoService.AlternarEstadoReceta(idReceta);
                TempData["Mensaje"] = "Estado de la receta actualizado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index", new { idRecetaProducto = idProducto });
        }

        [HttpPost]
        public ActionResult EliminarReceta(int idReceta, int idProducto)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _productoService.EliminarRecetaCompleta(idReceta);
                TempData["Mensaje"] = "Receta eliminada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
    }
}
