using System;
using System.Collections.Generic;

namespace Abstracciones.Models
{
    public class Caja
    {
        public int IdCaja { get; set; }
        public string NombreCaja { get; set; }
        public string EstadoCaja { get; set; }
        public bool Estado { get; set; }
        public ICollection<AperturaCaja> Aperturas { get; set; }
    }
}
