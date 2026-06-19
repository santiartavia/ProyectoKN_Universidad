using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class MesaAtendidaService : IMesaAtendidaService
    {
        private readonly IAuditoriaService _auditoria;
        private readonly IFechasLN _fechas;

        public MesaAtendidaService(IAuditoriaService auditoria, IFechasLN fechas)
        {
            _auditoria = auditoria;
            _fechas = fechas;
        }

        public void Registrar(int idMesa, int idEmpleado, int idPedido, string origenMesa)
        {
            using (var ctx = new ColibriDbContext())
            {
                var pedido = ctx.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido);
                if (pedido == null)
                    throw new KeyNotFoundException("La orden especificada no existe");

                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdEmpleado == idEmpleado);
                if (empleado == null)
                    throw new KeyNotFoundException("El empleado especificado no existe");
                if (!empleado.Estado)
                    throw new InvalidOperationException("No se pueden asignar mesas a un empleado inactivo");

                var mesa = ctx.Mesas.FirstOrDefault(m => m.IdMesa == idMesa && m.Estado);
                if (mesa == null)
                    throw new KeyNotFoundException("La mesa especificada no existe o está inactiva");

                bool yaAsignada = ctx.Pedidos.Any(p =>
                    p.IdPedido != idPedido && p.IdMesa == idMesa &&
                    p.EstadoPedido != "cancelado" && p.EstadoPedido != "entregado");
                if (yaAsignada)
                    throw new InvalidOperationException("La mesa ya está siendo utilizada en otra orden activa");

                pedido.IdMesa = idMesa;
                pedido.IdEmpleado = idEmpleado;
                mesa.EstadoMesa = "ocupada";
                ctx.SaveChanges();

                _auditoria.Registrar("Pedidos", idPedido, "ASIGNACION_MESA",
                    null,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { idMesa, idEmpleado, origenMesa }),
                    $"Mesa #{idMesa} asignada al pedido #{idPedido} (mesero #{idEmpleado})",
                    idEmpleado);
            }
        }

        public List<MesaAtendida> ObtenerMetricasPorEmpleado(int idEmpleado, DateTime fecha)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = from p in ctx.Pedidos
                            join m in ctx.Mesas on p.IdMesa equals m.IdMesa
                            join e in ctx.Empleados on p.IdEmpleado equals e.IdEmpleado
                            where p.IdEmpleado == idEmpleado
                                  && p.FechaHora.Year == fecha.Year
                                  && p.FechaHora.Month == fecha.Month
                                  && p.FechaHora.Day == fecha.Day
                            group new { p, m, e } by new { p.IdEmpleado, Fecha = System.Data.Entity.DbFunctions.TruncateTime(p.FechaHora) } into g
                            select new MesaAtendida
                            {
                                IdEmpleado = g.Key.IdEmpleado,
                                FechaHora = g.Key.Fecha.Value,
                                TotalMesas = g.Count(),
                                NombreEmpleado = g.FirstOrDefault().e.Nombre + " " + g.FirstOrDefault().e.Apellidos
                            };
                return query.ToList();
            }
        }

        public List<MesaAtendida> ObtenerMetricasPorSemana(int idEmpleado, DateTime fechaInicio, DateTime fechaFin)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = from p in ctx.Pedidos
                            join m in ctx.Mesas on p.IdMesa equals m.IdMesa
                            join e in ctx.Empleados on p.IdEmpleado equals e.IdEmpleado
                            where p.IdEmpleado == idEmpleado
                                  && p.FechaHora >= fechaInicio
                                  && p.FechaHora <= fechaFin
                            group new { p, m, e } by new { p.IdEmpleado, Fecha = System.Data.Entity.DbFunctions.TruncateTime(p.FechaHora) } into g
                            select new MesaAtendida
                            {
                                IdEmpleado = g.Key.IdEmpleado,
                                FechaHora = g.Key.Fecha.Value,
                                TotalMesas = g.Count(),
                                NombreEmpleado = g.FirstOrDefault().e.Nombre + " " + g.FirstOrDefault().e.Apellidos
                            };
                return query.ToList();
            }
        }

        public List<MesaAtendida> ObtenerMetricasGlobales(DateTime fecha)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = from p in ctx.Pedidos
                            join m in ctx.Mesas on p.IdMesa equals m.IdMesa
                            join e in ctx.Empleados on p.IdEmpleado equals e.IdEmpleado
                            where p.FechaHora.Year == fecha.Year
                                  && p.FechaHora.Month == fecha.Month
                                  && p.FechaHora.Day == fecha.Day
                            group new { p, m, e } by new { p.IdEmpleado, Fecha = System.Data.Entity.DbFunctions.TruncateTime(p.FechaHora) } into g
                            select new MesaAtendida
                            {
                                IdEmpleado = g.Key.IdEmpleado,
                                FechaHora = g.Key.Fecha.Value,
                                TotalMesas = g.Count(),
                                NombreEmpleado = g.FirstOrDefault().e.Nombre + " " + g.FirstOrDefault().e.Apellidos
                            };
                return query.OrderByDescending(q => q.TotalMesas).ToList();
            }
        }

        public List<MesaAtendida> ObtenerMetricasGlobalesSemana(DateTime fechaInicio, DateTime fechaFin)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = from p in ctx.Pedidos
                            join m in ctx.Mesas on p.IdMesa equals m.IdMesa
                            join e in ctx.Empleados on p.IdEmpleado equals e.IdEmpleado
                            where p.FechaHora >= fechaInicio
                                  && p.FechaHora <= fechaFin
                            group new { p, m, e } by new { p.IdEmpleado, Fecha = System.Data.Entity.DbFunctions.TruncateTime(p.FechaHora) } into g
                            select new MesaAtendida
                            {
                                IdEmpleado = g.Key.IdEmpleado,
                                FechaHora = g.Key.Fecha.Value,
                                TotalMesas = g.Count(),
                                NombreEmpleado = g.FirstOrDefault().e.Nombre + " " + g.FirstOrDefault().e.Apellidos
                            };
                return query.OrderByDescending(q => q.TotalMesas).ToList();
            }
        }
    }
}