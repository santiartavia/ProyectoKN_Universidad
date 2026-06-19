using Abstracciones.Models;
using System;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface ITurnoService
    {
        TurnoTrabajo Crear(int idEmpleado, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin,
                           string descripcion, int idUsuarioAdmin);
        List<TurnoTrabajo> ObtenerPorEmpleado(int idEmpleado);
        List<TurnoTrabajo> ObtenerPorFecha(DateTime fecha);
        List<Rol> ObtenerRoles();
        bool ValidarTraslape(int idEmpleado, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin);
        List<TurnoTrabajo> ListarTodos();
        List<TurnoTrabajo> ObtenerPorRango(DateTime fechaInicio, DateTime fechaFin);
        void Inactivar(int idTurno, int idUsuarioAdmin);
    }
}