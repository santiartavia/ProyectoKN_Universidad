using System;

namespace Abstracciones.Models
{
    public class CierrePeriodo
    {
        public int IdCierrePeriodo { get; set; }
        public int IdUsuario { get; set; }
        public string TipoPeriodo { get; set; }
        public int? Mes { get; set; }
        public int Anio { get; set; }
        public DateTime FechaCierre { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal TotalNotasCredito { get; set; }
        public decimal SaldoFinal { get; set; }
        public bool Estado { get; set; }
        public Usuario Usuario { get; set; }
    }
}
