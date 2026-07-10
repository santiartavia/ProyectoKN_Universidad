using Abstracciones.Models;
using System;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class InventarioViewModel
    {
        public List<Insumo> Insumos { get; set; }
        public List<Insumo> InsumosBajoStock { get; set; }
        public List<CategoriaInsumo> Categorias { get; set; }
        public List<Proveedor> Proveedores { get; set; }
        public List<BitacoraInventario> Bitacora { get; set; }
        public List<string> AccionesBitacora { get; set; }

        public string Busqueda { get; set; }
        public int? FiltroCategoria { get; set; }
        public int? FiltroUsuario { get; set; }
        public string FiltroAccion { get; set; }
        public string FiltroFechaInicio { get; set; }
        public string FiltroFechaFin { get; set; }

        public int InsumosActivos { get; set; }
        public int AlertasMinimas { get; set; }
        public int TotalCategorias { get; set; }

        public string Mensaje { get; set; }
        public string Error { get; set; }
        public string UltimoMovimiento { get; set; }

        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public const int TAMANIO_PAGINA = 10;
    }
}
