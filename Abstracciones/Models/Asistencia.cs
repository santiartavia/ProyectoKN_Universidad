using System;

namespace Abstracciones.Models
{
    public class Asistencia
    {
        public int IdAsistencia { get; set; }
        public int IdEmpleado { get; set; }
        public int? IdTurno { get; set; }
        public DateTime FechaHoraEntrada { get; set; }
        public DateTime? FechaHoraSalida { get; set; }
        public string Observaciones { get; set; }
        public bool Estado { get; set; }

        public Empleado Empleado { get; set; }
        public TurnoTrabajo Turno { get; set; }
    }
}