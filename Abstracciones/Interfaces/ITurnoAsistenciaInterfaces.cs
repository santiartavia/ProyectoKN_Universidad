using Abstracciones.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstracciones.Interfaces
{
    // Interfaces para Acceso a Datos (AD)
    public interface ITurnoTrabajoAD
    {
        int CrearTurno(TurnoTrabajoDto turno);
        bool ExisteTurno(int idEmpleado, DateTime fechaTurno);
    }

    public interface IAsistenciaAD
    {
        int RegistrarEntrada(AsistenciaDto asistencia);
        int RegistrarSalida(AsistenciaDto asistencia);
        AsistenciaDto ObtenerAsistenciaPendiente(int idEmpleado);
    }

    // Interfaces para Lógica de Negocios (LN)
    public interface ITurnoTrabajoLN
    {
        int CrearTurno(TurnoTrabajoDto turno);
    }

    public interface IAsistenciaLN
    {
        int RegistrarEntrada(int idEmpleado);
        int RegistrarSalida(int idEmpleado);
    }
}