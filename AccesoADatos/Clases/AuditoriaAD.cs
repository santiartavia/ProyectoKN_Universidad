using Abstracciones.Interfaces;
using Abstracciones.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoADatos.Clases
{
    public class AuditoriaAD : IAuditoriaAD
    {
        public void Registrar(BitacoraRRHH registro)
        {
            using (var ctx = new ColibriDbContext())
            {
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
                var query = ctx.BitacoraRRHH.Include(b => b.Usuario).AsQueryable();

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