using Abstracciones.Interfaces;
using Abstracciones.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace AccesoADatos.Clases
{
    public class HoraExtraAD : IHoraExtraAD
    {
        public Asistencia ObtenerAsistenciaConEmpleado(int idAsistencia)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Asistencias.Include(a => a.Empleado)
                          .FirstOrDefault(a => a.IdAsistencia == idAsistencia && a.Estado);
            }
        }

        public HoraExtra ObtenerHoraExtraConEmpleado(int idHoraExtra)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.HorasExtra.Include(h => h.Asistencia.Empleado)
                          .FirstOrDefault(h => h.IdHoraExtra == idHoraExtra && h.Estado);
            }
        }

        public HoraExtra Registrar(HoraExtra horaExtra)
        {
            using (var ctx = new ColibriDbContext())
            {
                ctx.HorasExtra.Add(horaExtra);
                ctx.SaveChanges();
                return horaExtra;
            }
        }

        public HoraExtra Ajustar(int idHoraExtra, decimal cantidadHoras, decimal montoCalculado, string motivoAjuste)
        {
            using (var ctx = new ColibriDbContext())
            {
                var registro = ctx.HorasExtra.Find(idHoraExtra);
                if (registro != null)
                {
                    registro.CantidadHoras = cantidadHoras;
                    registro.MontoCalculado = montoCalculado;
                    registro.MotivoAjuste = motivoAjuste;
                    ctx.SaveChanges();
                }
                return registro;
            }
        }

        public List<HoraExtra> ObtenerRegistrosSemana(int idEmpleado, DateTime fechaInicio, DateTime fechaFin)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.HorasExtra.Include(h => h.Asistencia)
                          .Where(h => h.Asistencia.IdEmpleado == idEmpleado
                                   && h.Estado
                                   && h.FechaRegistro >= fechaInicio
                                   && h.FechaRegistro <= fechaFin)
                          .ToList();
            }
        }

        // --- MÉTODOS QUE FALTABAN PARA CUMPLIR LA INTERFAZ ---

        public List<HoraExtra> ListarPorAsistencia(int idAsistencia)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.HorasExtra.Include(h => h.Asistencia)
                    .Where(h => h.IdAsistencia == idAsistencia && h.Estado)
                    .ToList();
            }
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