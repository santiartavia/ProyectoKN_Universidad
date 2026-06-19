using System;

namespace Abstracciones.Models
{
    public class Pedido
    {
        public int IdPedido { get; set; }
        public int? IdMesa { get; set; }
        public int IdEmpleado { get; set; }
        public string TipoServicio { get; set; }
        public byte? CantidadComensales { get; set; }
        public string EstadoPedido { get; set; }
        public DateTime FechaHora { get; set; }
        public string Observaciones { get; set; }
        public bool Estado { get; set; }
    }
}