using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class TurnoService : ITurnoService
    {
        private readonly IAuditoriaService _auditoria;
        private readonly IFechasLN _fechas;

        public TurnoService(IAuditoriaService auditoria, IFechasLN fechas)
        {
            _auditoria = auditoria;
            _fechas = fechas;
        }

        public TurnoTrabajo Crear(int idEmpleado, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin,
                                   string descripcion, int idUsuarioAdmin)
        {
            if (horaFin <= horaInicio)
                throw new ArgumentException("La hora de fin debe ser posterior a la hora de inicio");

            if (ValidarTraslape(idEmpleado, fecha, horaInicio, horaFin))
                throw new InvalidOperationException("El empleado ya tiene un turno asignado en ese rango de horario");

            using (var ctx = new ColibriDbContext())
            {
                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdEmpleado == idEmpleado && e.Estado);
                if (empleado == null)
                    throw new KeyNotFoundException("Empleado no encontrado o inactivo");

                var turno = new TurnoTrabajo
                {
                    IdEmpleado = idEmpleado,
                    FechaTurno = fecha,
                    HoraInicio = horaInicio,
                    HoraFin = horaFin,
                    Descripcion = descripcion,
                    Estado = true
                };
                ctx.TurnosTrabajo.Add(turno);
                ctx.SaveChanges();

                _auditoria.Registrar("Turnos_Trabajo", turno.IdTurno, "INSERT",
                    null,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        turno.IdEmpleado, turno.FechaTurno,
                        turno.HoraInicio, turno.HoraFin, turno.Descripcion
                    }),
                    $"Turno creado para empleado #{idEmpleado} el {fecha:yyyy-MM-dd} ({horaInicio:hh\\:mm}-{horaFin:hh\\:mm})",
                    idUsuarioAdmin);

                return turno;
            }
        }

        public List<TurnoTrabajo> ObtenerPorEmpleado(int idEmpleado)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.TurnosTrabajo.Include(t => t.Empleado)
                    .Where(t => t.IdEmpleado == idEmpleado && t.Estado)
                    .OrderByDescending(t => t.FechaTurno)
                    .ToList();
            }
        }

        public List<TurnoTrabajo> ObtenerPorFecha(DateTime fecha)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.TurnosTrabajo.Include(t => t.Empleado)
                    .Where(t => t.FechaTurno == fecha && t.Estado)
                    .OrderBy(t => t.HoraInicio)
                    .ToList();
            }
        }

        public bool ValidarTraslape(int idEmpleado, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.TurnosTrabajo.Any(t =>
                    t.IdEmpleado == idEmpleado &&
                    t.FechaTurno == fecha &&
                    t.Estado &&
                    ((horaInicio >= t.HoraInicio && horaInicio < t.HoraFin) ||
                     (horaFin > t.HoraInicio && horaFin <= t.HoraFin) ||
                     (horaInicio <= t.HoraInicio && horaFin >= t.HoraFin)));
            }
        }

        public List<Rol> ObtenerRoles()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Roles.Where(r => r.Estado).ToList();
            }
        }

        public List<TurnoTrabajo> ListarTodos()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.TurnosTrabajo.Include(t => t.Empleado)
                    .Where(t => t.Estado)
                    .OrderByDescending(t => t.FechaTurno)
                    .ThenBy(t => t.HoraInicio)
                    .ToList();
            }
        }

        public List<TurnoTrabajo> ObtenerPorRango(DateTime fechaInicio, DateTime fechaFin)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.TurnosTrabajo.Include(t => t.Empleado)
                    .Where(t => t.Estado && t.FechaTurno >= fechaInicio && t.FechaTurno <= fechaFin)
                    .OrderBy(t => t.FechaTurno)
                    .ThenBy(t => t.HoraInicio)
                    .ToList();
            }
        }

        public void Inactivar(int idTurno, int idUsuarioAdmin)
        {
            using (var ctx = new ColibriDbContext())
            {
                var turno = ctx.TurnosTrabajo.FirstOrDefault(t => t.IdTurno == idTurno);
                if (turno == null)
                    throw new KeyNotFoundException("Turno no encontrado");

                turno.Estado = false;
                ctx.SaveChanges();

                _auditoria.Registrar("Turnos_Trabajo", idTurno, "DESACTIVACION",
                    "Estado: Activo", "Estado: Inactivo",
                    $"Turno #{idTurno} desactivado",
                    idUsuarioAdmin);
            }
        }
    }
}