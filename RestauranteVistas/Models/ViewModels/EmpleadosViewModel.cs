using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class EmpleadosViewModel
    {
        public List<Empleado> Empleados { get; set; }
        public List<TurnoTrabajo> Turnos { get; set; }
        public List<Vacacion> Vacaciones { get; set; }
        public List<Vacacion> VacacionesAprobadas { get; set; }
        public List<HoraExtra> HorasExtra { get; set; }
        public List<BitacoraRRHH> Bitacora { get; set; }
        public List<Rol> Roles { get; set; }
        public int EmpleadosActivos { get; set; }
        public int TurnosProgramados { get; set; }
        public string UltimoMovimiento { get; set; }
        public string Mensaje { get; set; }
        public string Error { get; set; }

        public int? FiltroIdEmpleado { get; set; }
        public string FiltroFechaInicio { get; set; }
        public string FiltroFechaFin { get; set; }
        public string FiltroAccion { get; set; }

        public List<string> AccionesBitacora { get; set; }

        public List<Asistencia> Asistencias { get; set; }
        public List<Asistencia> AsistenciasPendientes { get; set; }

        public List<MesaAtendida> MetricasMesas { get; set; }
        public List<MesaAtendida> MetricasMesasSemana { get; set; }
        public string TerminoBusqueda { get; set; }
    }
}