using Abstracciones.Models;
using System;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface IVacacionService
    {
        Vacacion Solicitar(int idEmpleado, DateTime fechaInicio, DateTime fechaFin, int idUsuarioSolicitante);
        Vacacion Aprobar(int idVacacion, int idAprobador);
        Vacacion Rechazar(int idVacacion, int idAprobador, string motivoRechazo);
        List<Vacacion> ListarPorEmpleado(int idEmpleado);
        List<Vacacion> ListarPendientes();
        decimal CalcularSaldoVacacional(int idEmpleado);
        List<Vacacion> ListarTodas();
        List<Vacacion> ListarPorFiltros(int? idEmpleado, DateTime? fechaInicio, DateTime? fechaFin, string estado);
    }
}