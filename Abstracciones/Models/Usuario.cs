using System;

namespace Abstracciones.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string NombreUsuario { get; set; }
        public string Correo { get; set; }
        public string PasswordHash { get; set; }
        public bool CambioPasswordRequerido { get; set; }
        public byte IntentosFallidos { get; set; }
        public bool Bloqueado { get; set; }
        public DateTime? FechaUltimoAcceso { get; set; }
        public bool Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaPassword { get; set; }
        public DateTime? FechaAvisoPassword { get; set; }
        public string UltimoCambioPasswordIp { get; set; }
        public string UltimoCambioPasswordDispositivo { get; set; }

        public Rol Rol { get; set; }
    }
}
