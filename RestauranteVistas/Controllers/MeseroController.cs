using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Services;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Mesero", "Administrador" })]
    public class MeseroController : Controller
    {
        private readonly MesaAtendidaService _mesaAtendidaService;
        private ColibriDbContext db = new ColibriDbContext();

        public MeseroController()
        {
            var fechas = new FechasLN();
            var auditoria = new AuditoriaService(fechas);
            _mesaAtendidaService = new MesaAtendidaService(auditoria, fechas);
        }

        public ActionResult Index()
        {
            int idEmpleado = 0;

            if (Session["UsuarioId"] != null)
            {
                idEmpleado = Convert.ToInt32(Session["UsuarioId"]);
            }

            ViewBag.PedidosListos = db.Pedidos
                .Where(p => p.Estado == true &&
                            p.EstadoPedido == "listo" &&
                            (idEmpleado == 0 || p.IdEmpleado == idEmpleado))
                .OrderByDescending(p => p.FechaHora)
                .ToList();

            ViewBag.Detalles = db.DetallePedidos.Where(d => d.Estado == true).ToList();
            ViewBag.Productos = db.Productos.Where(p => p.Estado == true).ToList();

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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}