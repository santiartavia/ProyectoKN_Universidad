using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Services;
using System;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Mesero", "Administrador" })]
    public class MeseroController : Controller
    {
        private readonly MesaAtendidaService _mesaAtendidaService;

        public MeseroController()
        {
            var fechas = new FechasLN();
            var auditoria = new AuditoriaService(fechas);
            _mesaAtendidaService = new MesaAtendidaService(auditoria, fechas);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AsignarMesa(int idMesa, int idPedido, string origenMesa)
        {
            var idEmpleado = Session["UsuarioId"];
            if (idEmpleado == null)
                return RedirectToAction("Index", "Login");

            try
            {
                _mesaAtendidaService.Registrar(idMesa, (int)idEmpleado, idPedido, origenMesa ?? "Salon");
                TempData["Mensaje"] = "GES-004: Mesa asignada correctamente a la orden.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
    }
}
