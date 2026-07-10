using System;

namespace Abstracciones.Models
{
    public class AperturaCaja
    {
        public int IdApertura { get; set; }
        public int IdCaja { get; set; }
        public int IdCajero { get; set; }
        public decimal MontoInicial { get; set; }
        public DateTime FechaApertura { get; set; }
        public string Observaciones { get; set; }
        public bool Estado { get; set; }
        public Caja Caja { get; set; }
        public Empleado Cajero { get; set; }
    }
}
