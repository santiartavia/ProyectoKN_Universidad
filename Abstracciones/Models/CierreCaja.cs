using System;

namespace Abstracciones.Models
{
    public class CierreCaja
    {
        public int IdCierre { get; set; }
        public int IdCajero { get; set; }
        public int IdApertura { get; set; }
        public DateTime FechaCierre { get; set; }
        public decimal MontoApertura { get; set; }
        public decimal TotalEfectivo { get; set; }
        public decimal TotalSinpe { get; set; }
        public decimal TotalTarjeta { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal SaldoEsperado { get; set; }
        public decimal SaldoReal { get; set; }
        public bool Descuadre { get; set; }
        public bool Estado { get; set; }
        public Empleado Cajero { get; set; }
        public AperturaCaja Apertura { get; set; }
    }
}
