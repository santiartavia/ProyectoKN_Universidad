using System;

namespace Abstracciones.Models
{
    public class BitacoraPdv
    {
        public int IdRegistro { get; set; }
        public int IdCaja { get; set; }
        public int IdUsuarioCajero { get; set; }
        public string AccionOperativa { get; set; }
        public int? IdVenta { get; set; }
        public string Detalle { get; set; }
        public DateTime FechaHora { get; set; }
        public bool Estado { get; set; }
    }
}