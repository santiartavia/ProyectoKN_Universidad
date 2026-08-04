using System;

namespace Abstracciones.Models
{
    public class DetalleVenta
    {
        public int IdDetalleVenta { get; set; }
        public int IdVenta { get; set; }
        public int IdProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal SubtotalItem { get; set; }
        public string ObservacionesItem { get; set; }
        public bool Estado { get; set; }

        public Venta Venta { get; set; }
        public Producto Producto { get; set; }
    }
}