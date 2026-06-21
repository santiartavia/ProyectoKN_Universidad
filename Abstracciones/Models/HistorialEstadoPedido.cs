using System;

namespace Abstracciones.Models
{
    public class HistorialEstadoPedido
    {
        public int IdHistorial { get; set; }
        public int IdPedido { get; set; }
        public string EstadoAnterior { get; set; }
        public string EstadoNuevo { get; set; }
        public string UsuarioResponsable { get; set; }
        public DateTime FechaHoraCambio { get; set; }
        public string Detalle { get; set; }
        public bool Estado { get; set; }
    }
}