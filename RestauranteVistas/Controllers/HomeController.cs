using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Services;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
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
            using (var db = new AccesoADatos.ColibriDbContext())
            {
                var hoy = new FechasLN().ObtenerFechaActual().Date;
                var manana = hoy.AddDays(1);
                ViewBag.VentasHoy = db.Ventas.Where(v => v.Estado && v.EstadoVenta == "completada" && v.FechaHora >= hoy && v.FechaHora < manana)
                    .Sum(v => (decimal?)v.TotalCobrado) ?? 0m;
                ViewBag.PedidosActivos = db.Pedidos.Count(p => p.Estado && p.EstadoPedido != "cancelado" && p.EstadoPedido != "finalizado");
                ViewBag.MesasOcupadas = db.Mesas.Count(m => m.Estado && m.EstadoMesa == "ocupada");
                ViewBag.StockBajo = db.Insumos.Count(i => i.Estado && i.StockActual <= i.StockMinimo);
                ViewBag.UltimosMovimientos = db.BitacoraRRHH.OrderByDescending(b => b.FechaHora).Take(8).ToList();
            }
            ViewBag.TotalEmpleados = _empleadoService.ListarActivos().Count;
            return View();
        }
    }
}
