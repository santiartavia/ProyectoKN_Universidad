using System;

namespace Abstracciones.Models
{
    public class SubcuentaVenta
    {
        public int IdSubcuenta { get; set; }
        public int IdVenta { get; set; }
        public string NombreSubcuenta { get; set; }
        public decimal Subtotal { get; set; }
        public bool Pagada { get; set; }
        public DateTime FechaOperacion { get; set; }
        public bool Estado { get; set; }
    }
}