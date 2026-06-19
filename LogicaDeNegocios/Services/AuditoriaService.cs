using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly IFechasLN _fechas;

        public AuditoriaService(IFechasLN fechas)
        {
            _fechas = fechas;
        }

        public void Registrar(string tablaAfectada, int idRegistroAfectado, string accion,
                               string valorAnterior, string valorNuevo, string detalle,
                               int idUsuario, string ipOrigen = null, string dispositivo = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var registro = new BitacoraRRHH
                {
                    IdUsuario = idUsuario,
                    TablaAfectada = tablaAfectada,
                    IdRegistroAfectado = idRegistroAfectado,
                    Accion = accion,
                    ValorAnterior = valorAnterior,
                    ValorNuevo = valorNuevo,
                    Detalle = detalle,
                    IpOrigen = ipOrigen,
                    Dispositivo = dispositivo,
                    FechaHora = _fechas.ObtenerFechaActual()
                };
                ctx.BitacoraRRHH.Add(registro);
                ctx.SaveChanges();
            }
        }

        public List<BitacoraRRHH> Consultar(int? idRegistroAfectado = null, int? idUsuario = null,
                                              DateTime? fechaInicio = null, DateTime? fechaFin = null,
                                              string accion = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.BitacoraRRHH.AsQueryable();

                if (idRegistroAfectado.HasValue)
                    query = query.Where(b => b.IdRegistroAfectado == idRegistroAfectado.Value);
                if (idUsuario.HasValue)
                    query = query.Where(b => b.IdUsuario == idUsuario.Value);
                if (fechaInicio.HasValue)
                    query = query.Where(b => b.FechaHora >= fechaInicio.Value);
                if (fechaFin.HasValue)
                    query = query.Where(b => b.FechaHora <= fechaFin.Value);
                if (!string.IsNullOrWhiteSpace(accion))
                    query = query.Where(b => b.Accion == accion);

                return query.OrderByDescending(b => b.FechaHora).ToList();
            }
        }

        public List<string> ObtenerAccionesDistinct()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.BitacoraRRHH
                    .Select(b => b.Accion)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList();
            }
        }
    }
}