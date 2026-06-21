using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class AsistenciaService : IAsistenciaService
    {
        private readonly IAuditoriaService _auditoria;
        private readonly IFechasLN _fechas;

        public AsistenciaService(IAuditoriaService auditoria, IFechasLN fechas)
        {
            _auditoria = auditoria;
            _fechas = fechas;
        }

        public Asistencia RegistrarEntrada(int idEmpleado)
        {
            using (var ctx = new ColibriDbContext())
            {
                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdEmpleado == idEmpleado && e.Estado);
                if (empleado == null)
                    throw new KeyNotFoundException("Empleado no encontrado o inactivo");

                var pendiente = ctx.Asistencias
                    .FirstOrDefault(a => a.IdEmpleado == idEmpleado && a.FechaHoraSalida == null && a.Estado);
                if (pendiente != null)
                    throw new InvalidOperationException("El empleado tiene una entrada registrada sin salida. Debe registrar la salida primero.");

                var hoy = _fechas.ObtenerFechaActual();
                var turno = ctx.TurnosTrabajo
                    .Where(t => t.IdEmpleado == idEmpleado && t.FechaTurno == hoy.Date && t.Estado)
                    .FirstOrDefault();

                var asistencia = new Asistencia
                {
                    IdEmpleado = idEmpleado,
                    IdTurno = turno?.IdTurno,
                    FechaHoraEntrada = hoy,
                    Estado = true
                };
                ctx.Asistencias.Add(asistencia);
                ctx.SaveChanges();

                _auditoria.Registrar("Asistencia", asistencia.IdAsistencia, "INSERT",
                    null,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { asistencia.IdEmpleado, asistencia.FechaHoraEntrada, turnoAsignado = turno?.IdTurno }),
                    $"Registro de entrada para empleado #{idEmpleado}",
                    1);

                return asistencia;
            }
        }

        public Asistencia RegistrarSalida(int idAsistencia)
        {
            using (var ctx = new ColibriDbContext())
            {
                var asistencia = ctx.Asistencias.Include(a => a.Empleado)
                    .FirstOrDefault(a => a.IdAsistencia == idAsistencia && a.Estado);
                if (asistencia == null)
                    throw new KeyNotFoundException("Registro de asistencia no encontrado");
                if (asistencia.FechaHoraSalida != null)
                    throw new InvalidOperationException("La salida ya fue registrada para esta asistencia");

                asistencia.FechaHoraSalida = _fechas.ObtenerFechaActual();
                ctx.SaveChanges();

                _auditoria.Registrar("Asistencia", asistencia.IdAsistencia, "UPDATE",
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { entrada = asistencia.FechaHoraEntrada }),
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { salida = asistencia.FechaHoraSalida, horasTrabajadas = (asistencia.FechaHoraSalida.Value - asistencia.FechaHoraEntrada).TotalHours }),
                    $"Registro de salida para empleado #{asistencia.IdEmpleado}",
                    1);

                return asistencia;
            }
        }

        public Asistencia RegistrarSalidaPorEmpleado(int idEmpleado)
        {
            using (var ctx = new ColibriDbContext())
            {
                var pendiente = ctx.Asistencias.Include(a => a.Empleado)
                    .FirstOrDefault(a => a.IdEmpleado == idEmpleado && a.FechaHoraSalida == null && a.Estado);
                if (pendiente == null)
                    throw new InvalidOperationException("El empleado no tiene una entrada pendiente de salida");

                pendiente.FechaHoraSalida = _fechas.ObtenerFechaActual();
                ctx.SaveChanges();

                return pendiente;
            }
        }

        public Asistencia ObtenerPorId(int idAsistencia)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Asistencias.Include(a => a.Empleado)
                    .FirstOrDefault(a => a.IdAsistencia == idAsistencia);
            }
        }

        public Asistencia ObtenerEntradaPendiente(int idEmpleado)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Asistencias
                    .FirstOrDefault(a => a.IdEmpleado == idEmpleado && a.FechaHoraSalida == null && a.Estado);
            }
        }

        public List<Asistencia> ListarPorEmpleado(int idEmpleado)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Asistencias.Include(a => a.Empleado)
                    .Where(a => a.IdEmpleado == idEmpleado && a.Estado)
                    .OrderByDescending(a => a.FechaHoraEntrada)
                    .ToList();
            }
        }

        public bool TieneAsistenciaEnFecha(int idEmpleado, DateTime fecha)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Asistencias.Any(a =>
                    a.IdEmpleado == idEmpleado &&
                    a.Estado &&
                    System.Data.Entity.DbFunctions.TruncateTime(a.FechaHoraEntrada) == fecha.Date);
            }
        }

        public List<Asistencia> ListarPendientes()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Asistencias.Include(a => a.Empleado)
                    .Where(a => a.FechaHoraSalida == null && a.Estado)
                    .OrderByDescending(a => a.FechaHoraEntrada)
                    .ToList();
            }
        }

        public List<Asistencia> ListarPorFecha(DateTime fecha)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Asistencias.Include(a => a.Empleado)
                    .Where(a => a.Estado &&
                        System.Data.Entity.DbFunctions.TruncateTime(a.FechaHoraEntrada) == fecha.Date)
                    .OrderByDescending(a => a.FechaHoraEntrada)
                    .ToList();
            }
        }
    }
}