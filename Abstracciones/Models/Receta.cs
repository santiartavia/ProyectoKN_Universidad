namespace Abstracciones.Models
{
    public class Receta
    {
        public int IdReceta { get; set; }
        public int IdProducto { get; set; }
        public bool Estado { get; set; }
        public Producto Producto { get; set; }
    }
}
