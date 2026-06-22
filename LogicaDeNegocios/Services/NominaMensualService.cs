using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class NominaMensualService : INominaMensualService
    {
        private readonly IAuditoriaService _auditoria;
        private readonly IFechasLN _fechas;

        public NominaMensualService(IAuditoriaService auditoria, IFechasLN fechas)
        {
            _auditoria = auditoria;
            _fechas = fechas;
        }

        public List<NominaMensual> CalcularPreview(int mes, int anio)
        {
            using (var ctx = new ColibriDbContext())
            {
                var empleados = ctx.Empleados.Where(e => e.Estado && e.IdEmpleado != 1).ToList();
                var resultado = new List<NominaMensual>();

                var primerDia = new DateTime(anio, mes, 1);
                var ultimoDia = primerDia.AddMonths(1).AddDays(-1);

                foreach (var emp in empleados)
                {
                    var asistencias = ctx.Asistencias
                        .Where(a => a.IdEmpleado == emp.IdEmpleado && a.Estado
                            && a.FechaHoraEntrada >= primerDia
                            && a.FechaHoraEntrada <= ultimoDia
                            && a.FechaHoraSalida != null)
                        .ToList();

                    var horasTrabajadas = asistencias
                        .Sum(a => (decimal)(a.FechaHoraSalida.Value - a.FechaHoraEntrada).TotalHours);

                    var diasTrabajados = asistencias
                        .Select(a => a.FechaHoraEntrada.Date)
                        .Distinct()
                        .Count();

                    var horasExtra = ctx.HorasExtra
                        .Where(h => h.Estado && h.Asistencia.IdEmpleado == emp.IdEmpleado
                            && h.FechaRegistro >= primerDia
                            && h.FechaRegistro <= ultimoDia)
                        .ToList();

                    var totalHorasExtra = horasExtra.Sum(h => h.CantidadHoras);
                    var montoHorasExtra = horasExtra.Sum(h => h.MontoCalculado ?? 0);

                    var vacaciones = ctx.Vacaciones
                        .Where(v => v.IdEmpleado == emp.IdEmpleado && v.Estado
                            && v.EstadoSolicitud == "aprobada"
                            && v.FechaInicio <= ultimoDia
                            && v.FechaFin >= primerDia)
                        .ToList();

                    var vacacionesPagadas = vacaciones.Sum(v =>
                    {
                        var inicio = v.FechaInicio < primerDia ? primerDia : v.FechaInicio;
                        var fin = v.FechaFin > ultimoDia ? ultimoDia : v.FechaFin;
                        return (decimal)(fin - inicio).Days + 1;
                    });

                    var horasPorDiaVacacion = 8m;
                    var montoVacaciones = vacacionesPagadas * horasPorDiaVacacion * emp.SalarioHora;

                    var salarioBase = emp.SalarioHora * horasTrabajadas;
                    var salarioBruto = salarioBase + montoHorasExtra + montoVacaciones;

                    var totalDiasMes = ultimoDia.Day;
                    var diasAusentes = totalDiasMes - diasTrabajados - (int)Math.Round(vacacionesPagadas);
                    if (diasAusentes < 0) diasAusentes = 0;

                    resultado.Add(new NominaMensual
                    {
                        IdEmpleado = emp.IdEmpleado,
                        Empleado = emp,
                        Mes = mes,
                        Anio = anio,
                        HorasTrabajadas = Math.Round(horasTrabajadas, 2),
                        HorasExtra = totalHorasExtra,
                        VacacionesPagadas = vacacionesPagadas,
                        HorasPorDiaVacacion = 8m,
                        MontoVacaciones = Math.Round(montoVacaciones, 2),
                        DiasTrabajados = diasTrabajados,
                        DiasAusentes = diasAusentes,
                        ValorHora = emp.SalarioHora,
                        SalarioBase = Math.Round(salarioBase, 2),
                        MontoHorasExtra = Math.Round(montoHorasExtra, 2),
                        Bonificaciones = 0,
                        SalarioBruto = Math.Round(salarioBruto, 2),
                        Estado = "pendiente"
                    });
                }

                return resultado;
            }
        }

        public NominaMensual Cerrar(int idEmpleado, int mes, int anio, string observaciones, int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                var existe = ctx.NominasMensuales.FirstOrDefault(n =>
                    n.IdEmpleado == idEmpleado && n.Mes == mes && n.Anio == anio && n.Estado == "cerrada");
                if (existe != null)
                    throw new InvalidOperationException("La nómina de este empleado para el período ya está cerrada.");

                var preview = CalcularPreview(mes, anio);
                var data = preview.FirstOrDefault(n => n.IdEmpleado == idEmpleado);
                if (data == null)
                    throw new KeyNotFoundException("No se encontraron datos para el empleado en este período.");

                var nomina = new NominaMensual
                {
                    IdEmpleado = idEmpleado,
                    Mes = mes,
                    Anio = anio,
                    HorasTrabajadas = data.HorasTrabajadas,
                    HorasExtra = data.HorasExtra,
                    VacacionesPagadas = data.VacacionesPagadas,
                    DiasTrabajados = data.DiasTrabajados,
                    DiasAusentes = data.DiasAusentes,
                    ValorHora = data.ValorHora,
                    SalarioBase = data.SalarioBase,
                    MontoHorasExtra = data.MontoHorasExtra,
                    HorasPorDiaVacacion = data.HorasPorDiaVacacion,
                    MontoVacaciones = data.MontoVacaciones,
                    Bonificaciones = data.Bonificaciones,
                    SalarioBruto = data.SalarioBruto,
                    Estado = "cerrada",
                    FechaCierre = _fechas.ObtenerFechaActual(),
                    Observaciones = observaciones,
                    CreadoPor = idUsuario,
                    CreatedAt = _fechas.ObtenerFechaActual(),
                    UpdatedAt = _fechas.ObtenerFechaActual()
                };

                ctx.NominasMensuales.Add(nomina);
                ctx.SaveChanges();

                _auditoria.Registrar("Nomina_Mensual", nomina.IdNominaMensual, "INSERT",
                    null,
                    JsonConvert.SerializeObject(new { nomina.IdEmpleado, nomina.Mes, nomina.Anio, nomina.Estado }),
                    $"Cierre de nómina mensual para empleado #{idEmpleado} ({mes}/{anio})",
                    idUsuario);

                return nomina;
            }
        }

        public void CerrarMes(int mes, int anio, string observaciones, int idUsuario)
        {
            var preview = CalcularPreview(mes, anio);
            foreach (var nomina in preview)
            {
                try
                {
                    Cerrar(nomina.IdEmpleado, mes, anio, observaciones, idUsuario);
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        public List<NominaMensual> ListarHistorial(int? mes, int? anio)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.NominasMensuales.Include(n => n.Empleado)
                    .Where(n => n.Estado == "cerrada").AsQueryable();

                if (mes.HasValue)
                    query = query.Where(n => n.Mes == mes.Value);
                if (anio.HasValue)
                    query = query.Where(n => n.Anio == anio.Value);

                return query.OrderByDescending(n => n.Anio).ThenByDescending(n => n.Mes).ThenBy(n => n.Empleado.Nombre).ToList();
            }
        }

        public NominaMensual ObtenerPorId(int id)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.NominasMensuales.Include(n => n.Empleado)
                    .FirstOrDefault(n => n.IdNominaMensual == id);
            }
        }
    }
}
