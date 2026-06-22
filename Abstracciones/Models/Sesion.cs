using System;

namespace Abstracciones.Models
{
    public class Sesion
    {
        public int IdSesion { get; set; }
        public int IdUsuario { get; set; }
        public string EstadoSesion { get; set; }
        public DateTime FechaHoraInicio { get; set; }
        public DateTime FechaHoraUltimaAct { get; set; }
        public DateTime? FechaHoraCierre { get; set; }
        public string DispositivoAcceso { get; set; }
        public string DireccionIp { get; set; }
        public string MotivoCierre { get; set; }

        public Usuario Usuario { get; set; }
    }
}
