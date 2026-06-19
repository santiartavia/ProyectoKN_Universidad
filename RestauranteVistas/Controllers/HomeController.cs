using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Services;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    public class HomeController : Controller
    {
        private readonly AuditoriaService _auditoria;
        private readonly EmpleadoService _empleadoService;

        public HomeController()
        {
            var fechas = new FechasLN();
            _auditoria = new AuditoriaService(fechas);
            _empleadoService = new EmpleadoService(_auditoria, fechas);
        }

        public ActionResult Index()
        {
            ViewBag.UltimosMovimientos = _auditoria.Consultar(null, null, null, null, null)
                .OrderByDescending(b => b.FechaHora).Take(8).ToList();
            ViewBag.TotalEmpleados = _empleadoService.ListarActivos().Count;
            return View();
        }
    }
}