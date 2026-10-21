using System;
namespace Abstracciones.Models
{
    public class SolicitudOperacion
    {
        public Guid IdSolicitud { get; set; }
        public int IdUsuario { get; set; }
        public string Operacion { get; set; }
        public DateTime FechaHora { get; set; }
    }
}
