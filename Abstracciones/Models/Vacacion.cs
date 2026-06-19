using System;

namespace Abstracciones.Models
{
    public class Vacacion
    {
        public int IdVacacion { get; set; }
        public int IdEmpleado { get; set; }
        public int? IdAprobador { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public decimal DiasSolicitados { get; set; }
        public string EstadoSolicitud { get; set; }
        public string MotivoRechazo { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public bool Estado { get; set; }

        public Empleado Empleado { get; set; }
        public Empleado Aprobador { get; set; }
    }
}