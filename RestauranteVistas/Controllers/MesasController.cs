using AccesoADatos;
using Abstracciones.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Mesero", "Administrador" })]
    public class MesasController : Controller
    {
        private ColibriDbContext db = new ColibriDbContext();

        public ActionResult Index()
        {
            var mesas = db.Mesas.OrderBy(m => m.NumeroMesa).ToList();
            return View(mesas);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string numeroMesa, byte capacidad)
        {
            if (string.IsNullOrWhiteSpace(numeroMesa))
            {
                TempData["Error"] = "El numero de mesa es obligatorio.";
                return View();
            }
            if (capacidad < 1)
            {
                TempData["Error"] = "La capacidad debe ser mayor a cero.";
                return View();
            }

            var existe = db.Mesas.Any(m => m.NumeroMesa == numeroMesa && m.Estado);
            if (existe)
            {
                TempData["Error"] = "Ya existe una mesa con ese numero.";
                return View();
            }

            db.Mesas.Add(new Mesa
            {
                NumeroMesa = numeroMesa,
                Capacidad = capacidad,
                EstadoMesa = "disponible",
                Estado = true
            });
            db.SaveChanges();

            TempData["Mensaje"] = "Mesa creada correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var mesa = db.Mesas.Find(id);
            if (mesa == null)
            {
                TempData["Error"] = "La mesa no existe.";
                return RedirectToAction("Index");
            }
            return View(mesa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string numeroMesa, byte capacidad, string estadoMesa)
        {
            var mesa = db.Mesas.Find(id);
            if (mesa == null)
            {
                TempData["Error"] = "La mesa no existe.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(numeroMesa))
            {
                TempData["Error"] = "El numero de mesa es obligatorio.";
                return View(mesa);
            }
            if (capacidad < 1)
            {
                TempData["Error"] = "La capacidad debe ser mayor a cero.";
                return View(mesa);
            }

            var duplicado = db.Mesas.Any(m => m.NumeroMesa == numeroMesa && m.IdMesa != id && m.Estado);
            if (duplicado)
            {
                TempData["Error"] = "Ya existe otra mesa con ese numero.";
                return View(mesa);
            }

            string[] estadosValidos = new[] { "disponible", "ocupada", "reservada", "sucia", "inactiva" };
            if (!string.IsNullOrWhiteSpace(estadoMesa) && !estadosValidos.Contains(estadoMesa))
            {
                TempData["Error"] = "Estado de mesa no valido.";
                return View(mesa);
            }

            mesa.NumeroMesa = numeroMesa;
            mesa.Capacidad = capacidad;
            if (!string.IsNullOrWhiteSpace(estadoMesa))
                mesa.EstadoMesa = estadoMesa;

            db.SaveChanges();
            TempData["Mensaje"] = "Mesa actualizada correctamente.";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var mesa = db.Mesas.Find(id);
            if (mesa == null)
            {
                TempData["Error"] = "La mesa no existe.";
                return RedirectToAction("Index");
            }

            bool enUso = db.Pedidos.Any(p => p.IdMesa == id && p.Estado &&
                (p.EstadoPedido == "abierto" || p.EstadoPedido == "en_proceso" ||
                 p.EstadoPedido == "listo" || p.EstadoPedido == "entregado"));
            ViewBag.TienePedidosActivos = enUso;
            return View(mesa);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var mesa = db.Mesas.Find(id);
            if (mesa == null)
            {
                TempData["Error"] = "La mesa no existe.";
                return RedirectToAction("Index");
            }

            bool enUso = db.Pedidos.Any(p => p.IdMesa == id && p.Estado &&
                (p.EstadoPedido == "abierto" || p.EstadoPedido == "en_proceso" ||
                 p.EstadoPedido == "listo" || p.EstadoPedido == "entregado"));
            if (enUso)
            {
                TempData["Error"] = "No se puede eliminar la mesa porque tiene pedidos activos.";
                return RedirectToAction("Index");
            }

            mesa.Estado = false;
            db.SaveChanges();
            TempData["Mensaje"] = "Mesa eliminada correctamente.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
