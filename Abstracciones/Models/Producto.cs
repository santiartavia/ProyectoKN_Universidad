namespace Abstracciones.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }
        public int IdCategoriaProd { get; set; }
        public string NombreProducto { get; set; }
        public string Descripcion { get; set; }
        public decimal PrecioVenta { get; set; }
        public bool Disponible { get; set; }
        public bool Estado { get; set; }
    }
}