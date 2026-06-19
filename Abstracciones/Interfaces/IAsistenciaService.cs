using Abstracciones.Models;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface IAsistenciaService
    {
        Asistencia RegistrarEntrada(int idEmpleado);
        Asistencia RegistrarSalida(int idAsistencia);
        Asistencia RegistrarSalidaPorEmpleado(int idEmpleado);
        Asistencia ObtenerPorId(int idAsistencia);
        Asistencia ObtenerEntradaPendiente(int idEmpleado);
        List<Asistencia> ListarPorEmpleado(int idEmpleado);
        bool TieneAsistenciaEnFecha(int idEmpleado, System.DateTime fecha);
        List<Asistencia> ListarPendientes();
        List<Asistencia> ListarPorFecha(System.DateTime fecha);
    }
}