using Abstracciones.Models;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface IProductoService
    {
        List<Producto> ListarProductos(string busqueda = null, int? idCategoria = null);
        Producto ObtenerProducto(int idProducto);
        Producto RegistrarProducto(string nombre, string descripcion, decimal precio, int idCategoria, bool disponible);
        Producto ActualizarProducto(int idProducto, string nombre, string descripcion, decimal precio, int idCategoria, bool disponible);
        void DesactivarProducto(int idProducto);
        void ReactivarProducto(int idProducto);
        List<CategoriaProducto> ListarCategoriasProducto();

        Receta ObtenerRecetaPorProducto(int idProducto);
        List<RecetaInsumo> ObtenerRecetaInsumos(int idReceta);
        Receta CrearRecetaCompleta(int idProducto, int[] idInsumos, decimal[] cantidades);
        void AgregarInsumoReceta(int idReceta, int idInsumo, decimal cantidad);
        void EliminarInsumoReceta(int idReceta, int idInsumo);
        void EliminarRecetaCompleta(int idReceta);
        void AlternarEstadoReceta(int idReceta);
    }
}
