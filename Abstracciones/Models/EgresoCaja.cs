using System;

namespace Abstracciones.Models
{
    public class EgresoCaja
    {
        public int IdEgreso { get; set; }
        public int IdApertura { get; set; }
        public int IdUsuario { get; set; }
        public string CategoriaGasto { get; set; }
        public string Descripcion { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaHora { get; set; }
        public bool Estado { get; set; }
        public AperturaCaja Apertura { get; set; }
        public Usuario Usuario { get; set; }
    }
}
