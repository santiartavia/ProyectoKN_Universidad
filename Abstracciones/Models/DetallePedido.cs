namespace Abstracciones.Models
{
    public class DetallePedido
    {
        public int IdDetalle { get; set; }
        public int IdPedido { get; set; }
        public int IdProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public string ObservacionesItem { get; set; }
        public string EstadoItem { get; set; }
        public bool Estado { get; set; }
        public Pedido Pedido { get; set; }
        public Producto Producto { get; set; }
    }
}