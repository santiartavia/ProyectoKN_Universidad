using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class ProductosViewModel
    {
        public List<Producto> Productos { get; set; }
        public List<CategoriaProducto> CategoriasProducto { get; set; }
        public List<Insumo> Insumos { get; set; }
        public List<RecetaInsumo> RecetaInsumos { get; set; }
        public Receta RecetaActual { get; set; }
        public Producto ProductoSeleccionado { get; set; }

        public string Busqueda { get; set; }
        public int? FiltroCategoria { get; set; }

        public string Mensaje { get; set; }
        public string Error { get; set; }

        public int TotalProductos { get; set; }
        public int TotalCategorias { get; set; }

        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public const int TAMANIO_PAGINA = 10;
    }
}
