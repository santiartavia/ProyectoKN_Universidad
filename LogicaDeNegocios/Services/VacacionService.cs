using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class VacacionService : IVacacionService
    {
        private readonly IAuditoriaService _auditoria;
        private readonly IFechasLN _fechas;

        public VacacionService(IAuditoriaService auditoria, IFechasLN fechas)
        {
            _auditoria = auditoria;
            _fechas = fechas;
        }

        // Adapted to match your IVacacionService interface and previous controller
        public int Solicitar(Vacacion solicitud, int idUsuarioResponsable, bool ignorarAdvertenciaTurnos = false)
        {
            if (solicitud.FechaFin < solicitud.FechaInicio)
                throw new ArgumentException("La fecha de fin debe ser posterior o igual a la fecha de inicio");

            solicitud.DiasSolicitados = (decimal)(solicitud.FechaFin - solicitud.FechaInicio).Days + 1;
            if (solicitud.DiasSolicitados <= 0)
                throw new ArgumentException("El período de vacaciones debe ser de al menos un día");

            using (var ctx = new ColibriDbContext())
            {
                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdEmpleado == solicitud.IdEmpleado && e.Estado);
                if (empleado == null)
                    throw new KeyNotFoundException("Empleado no encontrado o inactivo");

                if (solicitud.DiasSolicitados > empleado.DiasVacacionesDisponibles)
                    throw new InvalidOperationException(
                        $"Saldo insuficiente. Disponible: {empleado.DiasVacacionesDisponibles}, Solicitado: {solicitud.DiasSolicitados}");

                bool tieneTurnos = ctx.TurnosTrabajo.Any(t =>
                    t.IdEmpleado == solicitud.IdEmpleado &&
                    t.Estado &&
                    t.FechaTurno >= solicitud.FechaInicio &&
                    t.FechaTurno <= solicitud.FechaFin);

                if (tieneTurnos && !ignorarAdvertenciaTurnos)
                    throw new InvalidOperationException(
                        "ADVERTENCIA_TURNOS: El empleado tiene turnos laborales activos en el rango de fechas solicitado. " +
                        "Confirme si desea continuar.");

                solicitud.EstadoSolicitud = "pendiente";
                solicitud.FechaSolicitud = _fechas.ObtenerFechaActual();
                solicitud.Estado = true;

                ctx.Vacaciones.Add(solicitud);
                ctx.SaveChanges();

                _auditoria.Registrar("Vacaciones", solicitud.IdVacacion, "SOLICITUD",
                    null,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { solicitud.IdEmpleado, solicitud.FechaInicio, solicitud.FechaFin, solicitud.DiasSolicitados }),
                    $"Solicitud de vacaciones para empleado #{solicitud.IdEmpleado} ({solicitud.FechaInicio:yyyy-MM-dd} a {solicitud.FechaFin:yyyy-MM-dd})",
                    idUsuarioResponsable);

                return solicitud.IdVacacion;
            }
        }

        public void Aprobar(int idVacacion, int idAprobador, int idUsuarioResponsable)
        {
            using (var ctx = new ColibriDbContext())
            {
                var vacacion = ctx.Vacaciones.Include(v => v.Empleado)
                    .FirstOrDefault(v => v.IdVacacion == idVacacion && v.Estado);

                if (vacacion == null)
                    throw new KeyNotFoundException("Solicitud de vacaciones no encontrada");
                if (vacacion.EstadoSolicitud != "pendiente")
                    throw new InvalidOperationException($"La solicitud ya fue {vacacion.EstadoSolicitud}");
                if (!vacacion.Empleado.Estado)
                    throw new InvalidOperationException("No se puede aprobar una solicitud de un empleado inactivo");

                bool tieneTurnos = ctx.TurnosTrabajo.Any(t =>
                    t.IdEmpleado == vacacion.IdEmpleado &&
                    t.Estado &&
                    t.FechaTurno >= vacacion.FechaInicio &&
                    t.FechaTurno <= vacacion.FechaFin);

                if (tieneTurnos)
                    throw new InvalidOperationException(
                        "El empleado cuenta con turnos laborales activos en el rango de fechas solicitado. " +
                        "Debe cancelar o modificar los turnos antes de aprobar la vacación.");

                vacacion.EstadoSolicitud = "aprobada";
                vacacion.IdAprobador = idAprobador;

                var empleado = ctx.Empleados.Find(vacacion.IdEmpleado);
                if (empleado != null)
                {
                    empleado.DiasVacacionesDisponibles -= vacacion.DiasSolicitados;
                }

                ctx.SaveChanges();

                _auditoria.Registrar("Vacaciones", vacacion.IdVacacion, "UPDATE",
                    "Estado: pendiente", "Estado: aprobada",
                    $"Vacaciones aprobadas para empleado #{vacacion.IdEmpleado} " +
                    $"({vacacion.FechaInicio:yyyy-MM-dd} a {vacacion.FechaFin:yyyy-MM-dd}). " +
                    $"Saldo restante: {empleado?.DiasVacacionesDisponibles}",
                    idUsuarioResponsable);
            }
        }

        public void Rechazar(int idVacacion, string motivoRechazo, int idUsuarioResponsable)
        {
            if (string.IsNullOrWhiteSpace(motivoRechazo))
                throw new ArgumentException("El motivo de rechazo es obligatorio");

            using (var ctx = new ColibriDbContext())
            {
                var vacacion = ctx.Vacaciones.Include(v => v.Empleado)
                    .FirstOrDefault(v => v.IdVacacion == idVacacion && v.Estado);
                if (vacacion == null)
                    throw new KeyNotFoundException("Solicitud de vacaciones no encontrada");
                if (vacacion.EstadoSolicitud != "pendiente")
                    throw new InvalidOperationException($"La solicitud ya fue {vacacion.EstadoSolicitud}");
                if (!vacacion.Empleado.Estado)
                    throw new InvalidOperationException("No se puede rechazar una solicitud de un empleado inactivo");

                vacacion.EstadoSolicitud = "rechazada";
                vacacion.IdAprobador = idUsuarioResponsable; // Using the responsible user as approver/rejector
                vacacion.MotivoRechazo = motivoRechazo;
                ctx.SaveChanges();

                _auditoria.Registrar("Vacaciones", vacacion.IdVacacion, "UPDATE",
                    "Estado: pendiente", $"Estado: rechazada. Motivo: {motivoRechazo}",
                    $"Vacaciones rechazadas para empleado #{vacacion.IdEmpleado}",
                    idUsuarioResponsable);
            }
        }

        public void CalcularSaldoAutomaticamente(CalculoVacacionesDto calculo, int idUsuarioResponsable)
        {
            using (var ctx = new ColibriDbContext())
            {
                var empleado = ctx.Empleados.Find(calculo.IdEmpleado);
                if (empleado == null) throw new Exception("Empleado no encontrado.");

                decimal diasGenerados = calculo.DiasLaborados * calculo.FactorAcumulacion;
                string saldoAnterior = empleado.DiasVacacionesDisponibles.ToString();

                empleado.DiasVacacionesDisponibles += diasGenerados;

                ctx.SaveChanges();

                _auditoria.Registrar("Empleados", calculo.IdEmpleado, "UPDATE",
                    saldoAnterior, empleado.DiasVacacionesDisponibles.ToString(),
                    "Actualización automática de saldo de vacaciones.",
                    idUsuarioResponsable);
            }
        }

        // Kept existing methods for listing/querying
        public List<Vacacion> ListarPorEmpleado(int idEmpleado)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Vacaciones.Include(v => v.Empleado).Include(v => v.Aprobador)
                    .Where(v => v.IdEmpleado == idEmpleado && v.Estado)
                    .OrderByDescending(v => v.FechaSolicitud)
                    .ToList();
            }
        }

        public List<Vacacion> ListarPendientes()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Vacaciones.Include(v => v.Empleado)
                    .Where(v => v.EstadoSolicitud == "pendiente" && v.Estado)
                    .OrderBy(v => v.FechaInicio)
                    .ToList();
            }
        }

        public decimal CalcularSaldoVacacional(int idEmpleado)
        {
            using (var ctx = new ColibriDbContext())
            {
                var empleado = ctx.Empleados.Find(idEmpleado);
                return empleado?.DiasVacacionesDisponibles ?? 0;
            }
        }

        public List<Vacacion> ListarTodas()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Vacaciones.Include(v => v.Empleado).Include(v => v.Aprobador)
                    .Where(v => v.Estado)
                    .OrderByDescending(v => v.FechaSolicitud)
                    .ToList();
            }
        }

        public List<Vacacion> ListarPorFiltros(int? idEmpleado, DateTime? fechaInicio, DateTime? fechaFin, string estado)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.Vacaciones.Include(v => v.Empleado).Include(v => v.Aprobador)
                    .Where(v => v.Estado).AsQueryable();

                if (idEmpleado.HasValue)
                    query = query.Where(v => v.IdEmpleado == idEmpleado.Value);
                if (fechaInicio.HasValue)
                    query = query.Where(v => v.FechaSolicitud >= fechaInicio.Value);
                if (fechaFin.HasValue)
                    query = query.Where(v => v.FechaSolicitud <= fechaFin.Value);
                if (!string.IsNullOrWhiteSpace(estado))
                    query = query.Where(v => v.EstadoSolicitud == estado);

                return query.OrderByDescending(v => v.FechaSolicitud).ToList();
            }
        }
    }
}