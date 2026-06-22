using System;
using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Models
{
    public class Vacacion
    {
        public int IdVacacion { get; set; }

        [Required(ErrorMessage = "El empleado es obligatorio")]
        public int IdEmpleado { get; set; }

        public int? IdAprobador { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaFin { get; set; }

        [Required]
        public decimal DiasSolicitados { get; set; }

        public string EstadoSolicitud { get; set; }
        public string MotivoRechazo { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public bool Estado { get; set; }

        public Empleado Empleado { get; set; }
        public Empleado Aprobador { get; set; }
    }

    public class CalculoVacacionesDto
    {
        public int IdEmpleado { get; set; }
        public int DiasLaborados { get; set; }
        public string PoliticaVacaciones { get; set; } // "Empresa" o "Ley"
        public decimal FactorAcumulacion { get; set; }
    }
}