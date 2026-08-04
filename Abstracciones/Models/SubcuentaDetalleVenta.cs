using System;

namespace Abstracciones.Models
{
    public class SubcuentaDetalleVenta
    {
        public int IdSubcuentaDetalle { get; set; }
        public int IdSubcuenta { get; set; }
        public int IdDetalleVenta { get; set; }
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
        public DateTime FechaOperacion { get; set; }
        public bool Estado { get; set; }
    }
}