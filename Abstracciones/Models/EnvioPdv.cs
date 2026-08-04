using System;

namespace Abstracciones.Models
{
    public class EnvioPdv
    {
        public int IdEnvio { get; set; }
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public string Destino { get; set; }
        public string Prioridad { get; set; }
        public string EstadoEnvio { get; set; }
        public string CodigoConfirmacion { get; set; }
        public string Observaciones { get; set; }
        public string MotivoReenvio { get; set; }
        public DateTime FechaHoraEnvio { get; set; }
    }
}