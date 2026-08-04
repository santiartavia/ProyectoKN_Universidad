using AccesoADatos;
using RestauranteVistas.Filters;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
    public class BitacoraPdvController : Controller
    {
        private ColibriDbContext db = new ColibriDbContext();

        [HttpGet]
        public ActionResult Index(int? idUsuario, int? idCaja, string accion, string fechaInicio, string fechaFin)
        {
            var query = db.BitacoraPdv.AsQueryable();

            if (idUsuario.HasValue)
                query = query.Where(b => b.IdUsuarioCajero == idUsuario.Value);
            if (idCaja.HasValue)
                query = query.Where(b => b.IdCaja == idCaja.Value);
            if (!string.IsNullOrWhiteSpace(accion))
                query = query.Where(b => b.AccionOperativa == accion);
            if (!string.IsNullOrWhiteSpace(fechaInicio))
            {
                if (DateTime.TryParse(fechaInicio, out var fi))
                    query = query.Where(b => b.FechaHora >= fi);
            }
            if (!string.IsNullOrWhiteSpace(fechaFin))
            {
                if (DateTime.TryParse(fechaFin, out var ff))
                {
                    var ffSiguiente = ff.AddDays(1);
                    query = query.Where(b => b.FechaHora < ffSiguiente);
                }
            }

            var registros = query.OrderByDescending(b => b.FechaHora).ToList();

            var idsUsuarios = registros.Select(r => r.IdUsuarioCajero).Distinct().ToList();
            var cajeros = db.Empleados
                .Where(e => idsUsuarios.Contains(e.IdEmpleado))
                .ToDictionary(e => e.IdEmpleado, e => e.Nombre + " " + e.Apellidos);

            var idsCajas = registros.Select(r => r.IdCaja).Distinct().ToList();
            var cajas = db.Cajas.Where(c => idsCajas.Contains(c.IdCaja)).ToList();

            var acciones = db.BitacoraPdv.Select(b => b.AccionOperativa).Distinct().OrderBy(a => a).ToList();

            var modelo = new BitacoraPdvViewModel
            {
                Registros = registros.Select(r => new BitacoraPdvViewModel.RegistroPdv
                {
                    IdRegistro = r.IdRegistro,
                    NombreCaja = cajas.FirstOrDefault(c => c.IdCaja == r.IdCaja)?.NombreCaja ?? ("Caja #" + r.IdCaja),
                    NombreCajero = cajeros.ContainsKey(r.IdUsuarioCajero) ? cajeros[r.IdUsuarioCajero] : ("Empleado #" + r.IdUsuarioCajero),
                    AccionOperativa = r.AccionOperativa,
                    IdVenta = r.IdVenta,
                    Detalle = r.Detalle,
                    FechaHora = r.FechaHora
                }).ToList(),
                Cajeros = cajeros,
                AccionesDisponibles = acciones,
                CajasDisponibles = db.Cajas.Where(c => c.Estado).OrderBy(c => c.NombreCaja).ToList(),
                FiltroIdUsuario = idUsuario,
                FiltroIdCaja = idCaja,
                FiltroAccion = accion,
                FiltroFechaInicio = fechaInicio,
                FiltroFechaFin = fechaFin
            };

            ViewBag.ErrorAutorizacion = TempData["ErrorAutorizacion"]?.ToString();
            return View(modelo);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}