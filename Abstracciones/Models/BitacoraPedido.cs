using System;

namespace Abstracciones.Models
{
    public class BitacoraPedido
    {
        public int IdRegistro { get; set; }
        public int IdUsuario { get; set; }
        public int IdPedido { get; set; }
        public string Accion { get; set; }
        public string EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; }
        public string Detalle { get; set; }
        public DateTime FechaHora { get; set; }
    }
}