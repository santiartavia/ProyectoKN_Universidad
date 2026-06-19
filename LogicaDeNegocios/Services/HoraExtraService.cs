using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class HoraExtraService : IHoraExtraService
    {
        private readonly IAuditoriaService _auditoria;
        private readonly IFechasLN _fechas;

        public HoraExtraService(IAuditoriaService auditoria, IFechasLN fechas)
        {
            _auditoria = auditoria;
            _fechas = fechas;
        }

        public HoraExtra Registrar(int idAsistencia, decimal cantidadHoras, decimal factorPago, int idUsuarioAdmin)
        {
            if (cantidadHoras <= 0)
                throw new ArgumentException("La cantidad de horas extra debe ser mayor a cero");

            using (var ctx = new ColibriDbContext())
            {
                var asistencia = ctx.Asistencias.Include(a => a.Empleado)
                    .FirstOrDefault(a => a.IdAsistencia == idAsistencia && a.Estado);
                if (asistencia == null)
                    throw new KeyNotFoundException("Registro de asistencia no encontrado");
                if (asistencia.FechaHoraSalida == null)
                    throw new InvalidOperationException("El empleado no tiene un registro de salida válido para esta asistencia");
                if (!asistencia.Empleado.Estado)
                    throw new InvalidOperationException("No se pueden registrar horas extra para un empleado inactivo");

                decimal tarifaHoraExtra = asistencia.Empleado.SalarioHora * factorPago;
                decimal montoExtra = cantidadHoras * tarifaHoraExtra;

                var horaExtra = new HoraExtra
                {
                    IdAsistencia = idAsistencia,
                    CantidadHoras = cantidadHoras,
                    FactorPago = factorPago,
                    MontoCalculado = montoExtra,
                    FechaRegistro = _fechas.ObtenerFechaActual(),
                    Estado = true
                };
                ctx.HorasExtra.Add(horaExtra);
                ctx.SaveChanges();

                _auditoria.Registrar("Horas_Extra", horaExtra.IdHoraExtra, "INSERT",
                    null,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { cantidadHoras, factorPago, montoExtra }),
                    $"Registro de {cantidadHoras} horas extra para asistencia #{idAsistencia}",
                    idUsuarioAdmin);

                return horaExtra;
            }
        }

        public HoraExtra Ajustar(int idHoraExtra, decimal cantidadHoras, string motivoAjuste, int idUsuarioAdmin)
        {
            if (cantidadHoras <= 0)
                throw new ArgumentException("La cantidad de horas extra debe ser mayor a cero");
            if (string.IsNullOrWhiteSpace(motivoAjuste))
                throw new ArgumentException("El motivo de ajuste es obligatorio");

            using (var ctx = new ColibriDbContext())
            {
                var horaExtra = ctx.HorasExtra.Include(h => h.Asistencia.Empleado)
                    .FirstOrDefault(h => h.IdHoraExtra == idHoraExtra && h.Estado);
                if (horaExtra == null)
                    throw new KeyNotFoundException("Registro de horas extra no encontrado");
                if (!horaExtra.Asistencia.Empleado.Estado)
                    throw new InvalidOperationException("No se pueden ajustar horas extra de un empleado inactivo");

                var valorAnterior = Newtonsoft.Json.JsonConvert.SerializeObject(
                    new { horaExtra.CantidadHoras, horaExtra.MontoCalculado });

                horaExtra.CantidadHoras = cantidadHoras;
                horaExtra.MontoCalculado = cantidadHoras * horaExtra.FactorPago * horaExtra.Asistencia.Empleado.SalarioHora;
                horaExtra.MotivoAjuste = motivoAjuste;
                ctx.SaveChanges();

                _auditoria.Registrar("Horas_Extra", idHoraExtra, "UPDATE",
                    valorAnterior,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { cantidadHoras, motivoAjuste }),
                    $"Ajuste de horas extra: {motivoAjuste}",
                    idUsuarioAdmin);

                return horaExtra;
            }
        }

        public List<HoraExtra> ListarPorAsistencia(int idAsistencia)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.HorasExtra.Include(h => h.Asistencia)
                    .Where(h => h.IdAsistencia == idAsistencia && h.Estado)
                    .ToList();
            }
        }

        public decimal CalcularMontoExtra(decimal cantidadHoras, decimal salarioHora, decimal factorPago)
        {
            return cantidadHoras * salarioHora * factorPago;
        }

        public List<HoraExtra> ListarTodas()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.HorasExtra.Include(h => h.Asistencia.Empleado)
                    .Where(h => h.Estado)
                    .OrderByDescending(h => h.FechaRegistro)
                    .ToList();
            }
        }

        public List<HoraExtra> ListarPorFiltros(int? idEmpleado, DateTime? fechaInicio, DateTime? fechaFin)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.HorasExtra.Include(h => h.Asistencia.Empleado)
                    .Where(h => h.Estado).AsQueryable();

                if (idEmpleado.HasValue)
                    query = query.Where(h => h.Asistencia.IdEmpleado == idEmpleado.Value);
                if (fechaInicio.HasValue)
                    query = query.Where(h => h.FechaRegistro >= fechaInicio.Value);
                if (fechaFin.HasValue)
                    query = query.Where(h => h.FechaRegistro <= fechaFin.Value);

                return query.OrderByDescending(h => h.FechaRegistro).ToList();
            }
        }
    }
}