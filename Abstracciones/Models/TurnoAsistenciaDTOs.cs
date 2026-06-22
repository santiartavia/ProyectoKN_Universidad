using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Abstracciones.Models
{
    public class TurnoTrabajoDto
    {
        public int IdTurno { get; set; }

        [Required(ErrorMessage = "El empleado es obligatorio")]
        public int IdEmpleado { get; set; }

        public string NombreEmpleado { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FechaTurno { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public TimeSpan HoraFin { get; set; }

        public string TipoTurno { get; set; }
        public decimal JornadaHoras { get; set; }
    }

    public class AsistenciaDto
    {
        public int IdAsistencia { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime FechaHoraEntrada { get; set; }
        public DateTime? FechaHoraSalida { get; set; }
        public decimal? HorasTrabajadas { get; set; }
        public decimal? HorasExtra { get; set; }
        public string Estado { get; set; }
    }
}