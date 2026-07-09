using System;

namespace Abstracciones.Models
{
    public class Empleado
    {
        public int IdEmpleado { get; set; }
        public int IdUsuario { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Telefono { get; set; }
        public string CorreoPersonal { get; set; }
        public string Direccion { get; set; }
        public decimal SalarioHora { get; set; }
        public decimal DiasVacacionesDisponibles { get; set; }
        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public DateTime? FechaReactivacion { get; set; }
        public string MotivoInactivacion { get; set; }
        public bool Estado { get; set; }

        public Usuario Usuario { get; set; }
    }
}