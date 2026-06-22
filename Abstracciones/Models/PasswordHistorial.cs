using System;

namespace Abstracciones.Models
{
    public class PasswordHistorial
    {
        public int IdHistorial { get; set; }
        public int IdUsuario { get; set; }
        public string PasswordHash { get; set; }
        public DateTime FechaCambio { get; set; }
        public string MetodoCambio { get; set; }
        public string Dispositivo { get; set; }
        public string DireccionIp { get; set; }

        public Usuario Usuario { get; set; }
    }
}
