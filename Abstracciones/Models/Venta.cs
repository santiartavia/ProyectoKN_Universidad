using System;

namespace Abstracciones.Models
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public int IdPedido { get; set; }
        public int? IdSubcuenta { get; set; }
        public int IdEmpleado { get; set; }
        public int? IdApertura { get; set; }
        public string TipoVenta { get; set; }
        public decimal TotalCobrado { get; set; }
        public decimal MontoRecibido { get; set; }
        public decimal Vuelto { get; set; }
        public string MetodoPago { get; set; }
        public string EstadoVenta { get; set; }
        public DateTime FechaHora { get; set; }
        public bool Estado { get; set; }
    }
}