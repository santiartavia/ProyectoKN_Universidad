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
    public class MiAsistenciaController : Controller
    {
        private readonly IEmpleadoService _empleadoService;
        private readonly IAsistenciaService _asistenciaService;
        private readonly IFechasLN _fechas;

        public MiAsistenciaController()
        {
            var fechas = new FechasLN();
            var auditoria = new AuditoriaService(fechas);
            _empleadoService = new EmpleadoService(auditoria, fechas);
            _asistenciaService = new AsistenciaService(auditoria, fechas);
            _fechas = fechas;
        }

        public ActionResult Index()
        {
            var empleado = ObtenerEmpleadoSesion();
            if (empleado == null) return RedirectToAction("Index", "Login");

            var modelo = new MiAsistenciaViewModel
            {
                Empleado = empleado,
                AsistenciasRecientes = _asistenciaService.ListarPorEmpleado(empleado.IdEmpleado)
                    .Take(20).ToList(),
                EntradaPendiente = _asistenciaService.ObtenerEntradaPendiente(empleado.IdEmpleado)
            };

            if (TempData["Mensaje"] != null)
                modelo.Mensaje = TempData["Mensaje"].ToString();
            if (TempData["Error"] != null)
                modelo.Error = TempData["Error"].ToString();

            return View(modelo);
        }

        [HttpPost]
        public ActionResult RegistrarEntrada()
        {
            var empleado = ObtenerEmpleadoSesion();
            if (empleado == null) return RedirectToAction("Index", "Login");

            try
            {
                _asistenciaService.RegistrarEntrada(empleado.IdEmpleado);
                TempData["Mensaje"] = "Entrada registrada correctamente.";
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

        [HttpPost]
        public ActionResult RegistrarSalida()
        {
            var empleado = ObtenerEmpleadoSesion();
            if (empleado == null) return RedirectToAction("Index", "Login");

            try
            {
                _asistenciaService.RegistrarSalidaPorEmpleado(empleado.IdEmpleado);
                TempData["Mensaje"] = "Salida registrada correctamente.";
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
