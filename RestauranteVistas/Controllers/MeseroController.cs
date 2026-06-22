using AccesoADatos;
using AccesoADatos.Clases;
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

            // CORRECCIÓN: Instanciamos el AD y se lo pasamos al Service
            var auditoriaAD = new AuditoriaAD();
            var auditoria = new AuditoriaService(auditoriaAD, fechas);

            _mesaAtendidaService = new MesaAtendidaService(auditoria, fechas);
        }

        public ActionResult Index()
        {
            ViewBag.PedidosListos = db.Pedidos
                .Where(p => p.Estado == true && p.EstadoPedido == "listo")
                .OrderByDescending(p => p.FechaHora)
                .ToList();

            ViewBag.PedidosEntregados = db.Pedidos
                .Where(p => p.Estado == true && p.EstadoPedido == "entregado")
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

        [HttpPost]
        public ActionResult EntregarPedido(int idPedido)
        {
            var pedido = db.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado == true);

            if (pedido == null)
            {
                TempData["Error"] = "El pedido no existe.";
                return RedirectToAction("Index");
            }

            if (pedido.EstadoPedido != "listo")
            {
                TempData["Error"] = "Solo se pueden entregar pedidos en estado listo.";
                return RedirectToAction("Index");
            }

            pedido.EstadoPedido = "entregado";
            pedido.FechaHoraEntrega = DateTime.Now;

            db.SaveChanges();

            TempData["Mensaje"] = "Pedido entregado correctamente. Queda habilitado para pago.";
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