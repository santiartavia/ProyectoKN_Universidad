using System;

namespace Abstracciones.Models
{
    public class NotaCredito
    {
        public int IdNotaCredito { get; set; }
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public string Motivo { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaHora { get; set; }
        public bool Estado { get; set; }
        public Venta Venta { get; set; }
        public Usuario Usuario { get; set; }
    }
}
