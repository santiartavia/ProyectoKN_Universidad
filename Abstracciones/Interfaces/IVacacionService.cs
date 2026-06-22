using Abstracciones.Models;
using System;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface IVacacionAD
    {
        decimal ObtenerSaldoVacaciones(int idEmpleado);
        void ActualizarSaldoVacaciones(int idEmpleado, decimal nuevoSaldo, int idUsuarioResponsable);
        int CrearSolicitud(Vacacion solicitud, int idUsuarioResponsable);
        void AprobarSolicitud(int idVacacion, int idAprobador, int idUsuarioResponsable);
        void RechazarSolicitud(int idVacacion, string motivoRechazo, int idUsuarioResponsable);
        bool ExistenTurnosActivos(int idEmpleado, DateTime fechaInicio, DateTime fechaFin);
    }

    // Renombrado para que coincida con tu inyección de dependencias en el Controller
    public interface IVacacionService
    {
        void CalcularSaldoAutomaticamente(CalculoVacacionesDto calculo, int idUsuarioResponsable);
        int Solicitar(Vacacion solicitud, int idUsuarioResponsable, bool ignorarAdvertenciaTurnos = false);
        void Aprobar(int idVacacion, int idAprobador, int idUsuarioResponsable);
        void Rechazar(int idVacacion, string motivoRechazo, int idUsuarioResponsable);

        // Faltaban estas firmas que tu servicio ya tiene implementadas:
        List<Vacacion> ListarPorEmpleado(int idEmpleado);
        List<Vacacion> ListarPendientes();
        decimal CalcularSaldoVacacional(int idEmpleado);
        List<Vacacion> ListarTodas();
        List<Vacacion> ListarPorFiltros(int? idEmpleado, DateTime? fechaInicio, DateTime? fechaFin, string estado);
    }
}