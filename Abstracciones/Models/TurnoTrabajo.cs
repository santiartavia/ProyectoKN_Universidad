using System;

namespace Abstracciones.Models
{
    public class TurnoTrabajo
    {
        public int IdTurno { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime FechaTurno { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string Descripcion { get; set; }
        public bool Estado { get; set; }

        public Empleado Empleado { get; set; }
    }
}