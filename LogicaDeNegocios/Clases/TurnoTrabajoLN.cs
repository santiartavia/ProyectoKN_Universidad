using Abstracciones.Interfaces;
using Abstracciones.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogicaDeNegocios.Clases
{
    public class TurnoTrabajoLN : ITurnoTrabajoLN
    {
        private readonly ITurnoTrabajoAD _turnoAD;

        public TurnoTrabajoLN(ITurnoTrabajoAD turnoAD)
        {
            _turnoAD = turnoAD;
        }

        public int CrearTurno(TurnoTrabajoDto turno)
        {
            // Escenario 2: Horarios válidos
            if (turno.HoraFin <= turno.HoraInicio)
            {
                throw new InvalidOperationException("La hora de fin debe ser mayor a la hora de inicio.");
            }

            // Escenario 3: No se permiten turnos duplicados
            if (_turnoAD.ExisteTurno(turno.IdEmpleado, turno.FechaTurno.Date))
            {
                throw new InvalidOperationException("El empleado ya tiene un turno asignado para esta fecha.");
            }

            turno.JornadaHoras = (decimal)(turno.HoraFin - turno.HoraInicio).TotalHours;
            turno.TipoTurno = DeterminarTipoTurno(turno.HoraInicio);

            return _turnoAD.CrearTurno(turno);
        }

        private string DeterminarTipoTurno(TimeSpan horaInicio)
        {
            if (horaInicio.Hours < 14) return "Mañana";
            if (horaInicio.Hours < 19) return "Tarde";
            return "Noche";
        }
    }
}