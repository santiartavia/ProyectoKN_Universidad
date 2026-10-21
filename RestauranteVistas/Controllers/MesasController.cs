using AccesoADatos;
using Abstracciones.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Mesero", "Administrador", "Cajero" })]
    public class MesasController : OperacionController
    {


        public ActionResult Index()
        {
            var mesas = db.Mesas.Where(m => m.Estado).OrderBy(m => m.NumeroMesa).ToList();

            var empleados = db.Empleados.Include(e => e.Usuario).Include(e => e.Usuario.Rol)
                .Where(e => e.Estado && e.Usuario.Estado && e.Usuario.Rol.Estado && e.Usuario.Rol.NombreRol == "Mesero")
                .OrderByDescending(e => e.Usuario != null && e.Usuario.Rol != null && e.Usuario.Rol.NombreRol == "Mesero")
                .ThenBy(e => e.Nombre)
                .ToList();
            ViewBag.Meseros = empleados;

            // Obtener mesero asignado por mesa (desde pedido activo o última asignación en bitácora)
            var asignaciones = new Dictionary<int, string>();
            var pedidosActivos = db.Pedidos
                .Where(p => p.Estado && p.IdMesa.HasValue &&
                            (p.EstadoPedido == "abierto" || p.EstadoPedido == "en_proceso" ||
                             p.EstadoPedido == "listo" || p.EstadoPedido == "entregado"))
                .OrderByDescending(p => p.FechaHora)
                .ToList();

            var empDict = empleados.GroupBy(e => e.IdEmpleado)
                .ToDictionary(g => g.Key, g => $"{g.First().Nombre} {g.First().Apellidos}");

            foreach (var p in pedidosActivos)
            {
                if (p.IdMesa.HasValue && !asignaciones.ContainsKey(p.IdMesa.Value))
                {
                    asignaciones[p.IdMesa.Value] = empDict.ContainsKey(p.IdEmpleado)
                        ? empDict[p.IdEmpleado]
                        : $"Empleado #{p.IdEmpleado}";
                }
            }

            foreach (var mesa in mesas.Where(m => m.IdEmpleadoAsignado.HasValue))
                asignaciones[mesa.IdMesa] = empDict.ContainsKey(mesa.IdEmpleadoAsignado.Value)
                    ? empDict[mesa.IdEmpleadoAsignado.Value] : "Mesero no disponible";

            ViewBag.Asignaciones = asignaciones;
            return View(mesas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AsignarMesero(int idMesa, int idEmpleado, bool reasignar = false)
        {
            if (idMesa <= 0 || idEmpleado <= 0)
            {
                TempData["Error"] = "Debe seleccionar una mesa y un mesero.";
                return RedirectToAction("Index");
            }

            var mesa = db.Mesas.FirstOrDefault(m => m.IdMesa == idMesa && m.Estado);
            if (mesa == null)
            {
                TempData["Error"] = "La mesa seleccionada no existe.";
                return RedirectToAction("Index");
            }

            // C5 Paso 3: Intentar asignar una mesa ocupada a un nuevo mesero -> error indicando que no está disponible
            if (mesa.EstadoMesa == "ocupada" && !reasignar)
            {
                TempData["Error"] = $"La mesa {mesa.NumeroMesa} ya se encuentra en uso (ocupada) y no está disponible para asignarse a otro mesero.";
                return RedirectToAction("Index");
            }

            var empleado = db.Empleados.FirstOrDefault(e => e.IdEmpleado == idEmpleado && e.Estado && e.Usuario.Estado && e.Usuario.Rol.Estado && e.Usuario.Rol.NombreRol == "Mesero");
            if (empleado == null)
            {
                TempData["Error"] = "El mesero seleccionado no existe o está inactivo.";
                return RedirectToAction("Index");
            }

            if (mesa.EstadoMesa != "disponible" && mesa.EstadoMesa != "ocupada")
                throw new ArgumentException("La mesa no está disponible para asignación.");
            string estadoAnterior = mesa.EstadoMesa;
            mesa.EstadoMesa = "ocupada";
            mesa.IdEmpleadoAsignado = empleado.IdEmpleado;
            foreach (var pedido in db.Pedidos.Where(p => p.IdMesa == idMesa && p.Estado && p.EstadoPedido != "cancelado" && p.EstadoPedido != "finalizado"))
                pedido.IdEmpleado = empleado.IdEmpleado;

            int uid = Session["UsuarioId"] as int? ?? empleado.IdUsuario;
            db.BitacoraRRHH.Add(new BitacoraRRHH
            {
                IdUsuario = uid,
                TablaAfectada = "Mesas",
                IdRegistroAfectado = mesa.IdMesa,
                Accion = "UPDATE",
                ValorAnterior = estadoAnterior,
                ValorNuevo = "mesero:" + empleado.IdEmpleado,
                Detalle = $"Mesa {mesa.NumeroMesa} asignada al mesero {empleado.Nombre} {empleado.Apellidos}",
                IpOrigen = Request.UserHostAddress,
                Dispositivo = Request.UserAgent,
                FechaHora = DateTime.Now
            });

            db.SaveChanges();

            TempData["Mensaje"] = $"Mesa {mesa.NumeroMesa} asignada al mesero {empleado.Nombre} {empleado.Apellidos}. Estado cambiado a En Uso (ocupada).";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LiberarMesa(int id)
        {
            var mesa = db.Mesas.FirstOrDefault(m => m.IdMesa == id && m.Estado);
            if (mesa == null)
            {
                TempData["Error"] = "La mesa no existe.";
                return RedirectToAction("Index");
            }

            bool tienePedidoEnCocina = db.Pedidos.Any(p => p.IdMesa == id && p.Estado &&
                (p.EstadoPedido != "cancelado" && p.EstadoPedido != "finalizado"));
            if (tienePedidoEnCocina)
            {
                TempData["Error"] = $"La mesa {mesa.NumeroMesa} tiene un pedido en preparación/listo en cocina. Finalice o cancele el pedido antes de liberarla.";
                return RedirectToAction("Index");
            }

            string estadoAnterior = mesa.EstadoMesa;
            mesa.EstadoMesa = "disponible";
            mesa.IdEmpleadoAsignado = null;

            int uid = Session["UsuarioId"] as int? ?? 1;
            db.BitacoraRRHH.Add(new BitacoraRRHH
            {
                IdUsuario = uid,
                TablaAfectada = "Mesas",
                IdRegistroAfectado = mesa.IdMesa,
                Accion = "UPDATE",
                ValorAnterior = estadoAnterior,
                ValorNuevo = "disponible",
                Detalle = $"Mesa {mesa.NumeroMesa} liberada y marcada como disponible.",
                IpOrigen = Request.UserHostAddress,
                Dispositivo = Request.UserAgent,
                FechaHora = DateTime.Now
            });

            db.SaveChanges();
            TempData["Mensaje"] = $"Mesa {mesa.NumeroMesa} liberada correctamente. Ahora se encuentra Disponible.";
            return RedirectToAction("Index");
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

            bool enUso = mesa.EstadoMesa == "ocupada" || db.Pedidos.Any(p => p.IdMesa == id && p.Estado &&
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

            bool enUso = mesa.EstadoMesa == "ocupada" || db.Pedidos.Any(p => p.IdMesa == id && p.Estado &&
                (p.EstadoPedido == "abierto" || p.EstadoPedido == "en_proceso" ||
                 p.EstadoPedido == "listo" || p.EstadoPedido == "entregado"));
            if (enUso)
            {
                TempData["Error"] = $"No se puede eliminar la mesa {mesa.NumeroMesa} porque se encuentra en uso (ocupada) o tiene pedidos activos. Libere la mesa primero.";
                return RedirectToAction("Index");
            }

            mesa.Estado = false;
            db.SaveChanges();
            TempData["Mensaje"] = $"Mesa {mesa.NumeroMesa} eliminada correctamente.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
