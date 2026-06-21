using Abstracciones.Interfaces;
using Abstracciones.Models;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Services;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador", "Mesero", "Cajero", "Cocinero" })]
    public class MisVacacionesController : Controller
    {
        private readonly IEmpleadoService _empleadoService;
        private readonly IVacacionService _vacacionService;
        private readonly IFechasLN _fechas;

        public MisVacacionesController()
        {
            var fechas = new FechasLN();
            var auditoria = new AuditoriaService(fechas);
            _empleadoService = new EmpleadoService(auditoria, fechas);
            _vacacionService = new VacacionService(auditoria, fechas);
            _fechas = fechas;
        }

        public ActionResult Index()
        {
            var empleado = ObtenerEmpleadoSesion();
            if (empleado == null) return RedirectToAction("Index", "Login");

            var detalle = _vacacionService.CalcularDetalleVacacional(empleado.IdEmpleado);
            var vacaciones = _vacacionService.ListarPorEmpleado(empleado.IdEmpleado);

            var modelo = new MisVacacionesViewModel
            {
                Empleado = empleado,
                Vacaciones = vacaciones,
                DiasAcumulados = detalle.Earned,
                DiasUsados = detalle.Used,
                DiasDisponibles = detalle.Available
            };

            if (TempData["Mensaje"] != null)
                modelo.Mensaje = TempData["Mensaje"].ToString();
            if (TempData["Error"] != null)
                modelo.Error = TempData["Error"].ToString();

            return View(modelo);
        }

        [HttpPost]
        public ActionResult Solicitar(string fechaInicio, string fechaFin)
        {
            var empleado = ObtenerEmpleadoSesion();
            if (empleado == null) return RedirectToAction("Index", "Login");

            try
            {
                var inicio = DateTime.Parse(fechaInicio);
                var fin = DateTime.Parse(fechaFin);
                _vacacionService.Solicitar(empleado.IdEmpleado, inicio, fin, empleado.IdUsuario);
                TempData["Mensaje"] = "Solicitud de vacaciones enviada correctamente. Espere la aprobación del administrador.";
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    msg += $" | [{inner.GetType().Name}] {inner.Message}";
                    inner = inner.InnerException;
                }
                TempData["Error"] = $"Error: {msg}";
            }
            return RedirectToAction("Index");
        }

        private Empleado ObtenerEmpleadoSesion()
        {
            if (Session["UsuarioId"] == null) return null;
            var idUsuario = (int)Session["UsuarioId"];
            return _empleadoService.ObtenerPorUsuarioId(idUsuario);
        }
    }
}
