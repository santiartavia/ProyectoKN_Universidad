using System;
using System.Linq;
using System.Web.Mvc;
using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using RestauranteVistas.Models.ViewModels;

namespace RestauranteVistas.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public ActionResult Entrar(string usuario, string password)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorLogin"] = "Debe ingresar usuario y contraseña.";
                return RedirectToAction("Index");
            }

            using (var ctx = new ColibriDbContext())
            {
                var user = ctx.Usuarios.Include("Rol")
                    .FirstOrDefault(u => u.NombreUsuario == usuario && u.Estado);

                if (user == null)
                {
                    TempData["ErrorLogin"] = "Usuario o contraseña incorrectos.";
                    return RedirectToAction("Index");
                }

                if (user.Bloqueado)
                {
                    TempData["ErrorLogin"] = "Cuenta bloqueada. Contacte al administrador.";
                    return RedirectToAction("Index");
                }

                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    Session["UsuarioId"] = user.IdUsuario;
                    Session["UsuarioNombre"] = user.NombreUsuario;
                    Session["RolNombre"] = user.Rol.NombreRol;
                    Session["RolId"] = user.IdRol;
                    var empTmp = ctx.Empleados.FirstOrDefault(ee => ee.IdUsuario == user.IdUsuario && ee.Estado);
                    if (empTmp != null)
                        Session["UsuarioNombre"] = $"{empTmp.Nombre} {empTmp.Apellidos}";
                    return RedirectToAction("CrearPassword");
                }

                var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                var passwordHash = Convert.ToBase64String(passwordBytes);

                if (user.PasswordHash != passwordHash)
                {
                    user.IntentosFallidos++;
                    if (user.IntentosFallidos >= 5)
                        user.Bloqueado = true;
                    if (user.IntentosFallidos >= 3)
                        user.CambioPasswordRequerido = true;
                    ctx.SaveChanges();

                    TempData["ErrorLogin"] = "Usuario o contraseña incorrectos.";
                    return RedirectToAction("Index");
                }

                user.IntentosFallidos = 0;
                user.FechaUltimoAcceso = DateTime.UtcNow;

                var fechas = new FechasLN();
                if (user.FechaPassword.HasValue &&
                    (fechas.ObtenerFechaActual() - user.FechaPassword.Value).TotalDays >= 90)
                {
                    user.CambioPasswordRequerido = true;
                }

                ctx.SaveChanges();

                Session["UsuarioId"] = user.IdUsuario;
                Session["UsuarioNombre"] = user.NombreUsuario;
                Session["RolNombre"] = user.Rol.NombreRol;
                Session["RolId"] = user.IdRol;
                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == user.IdUsuario && e.Estado);
                if (empleado != null)
                    Session["UsuarioNombre"] = $"{empleado.Nombre} {empleado.Apellidos}";

                if (user.CambioPasswordRequerido)
                    return RedirectToAction("CambiarPassword");

                var rol = user.Rol.NombreRol.ToLower();
                switch (rol)
                {
                    case "administrador":
                    case "admin":
                        return RedirectToAction("Index", "Home");
                    case "cajero":
                        return RedirectToAction("Index", "Cajero");
                    case "mesero":
                        return RedirectToAction("Index", "Mesero");
                    case "cocinero":
                        return RedirectToAction("Index", "Cocina");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }
        }

        [HttpGet]
        public ActionResult CrearPassword()
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        public ActionResult CrearPassword(string passwordNuevo, string passwordConfirmar)
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Index");

            if (string.IsNullOrWhiteSpace(passwordNuevo))
            {
                TempData["Error"] = "Debe ingresar una contraseña.";
                return View();
            }

            if (passwordNuevo != passwordConfirmar)
            {
                TempData["Error"] = "Las contraseñas no coinciden.";
                return View();
            }

            if (passwordNuevo.Length < 6)
            {
                TempData["Error"] = "La contraseña debe tener al menos 6 caracteres.";
                return View();
            }

            using (var ctx = new ColibriDbContext())
            {
                var idUsuario = (int)Session["UsuarioId"];
                var user = ctx.Usuarios.Include("Rol").FirstOrDefault(u => u.IdUsuario == idUsuario);
                if (user == null)
                    return RedirectToAction("Logout");

                user.PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(passwordNuevo));
                user.CambioPasswordRequerido = false;
                user.FechaPassword = new FechasLN().ObtenerFechaActual();
                ctx.SaveChanges();

                TempData["Mensaje"] = "Contraseña creada correctamente.";
                var rol = user.Rol.NombreRol.ToLower();
                switch (rol)
                {
                    case "administrador":
                    case "admin":
                        return RedirectToAction("Index", "Home");
                    case "cajero":
                        return RedirectToAction("Index", "Cajero");
                    case "mesero":
                        return RedirectToAction("Index", "Mesero");
                    case "cocinero":
                        return RedirectToAction("Index", "Cocina");
                    default:
                        return RedirectToAction("Index", "Home");
                }
            }
        }

        [HttpGet]
        public ActionResult CambiarPassword()
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        public ActionResult CambiarPassword(string passwordActual, string passwordNuevo, string passwordConfirmar)
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Index");

            if (string.IsNullOrWhiteSpace(passwordActual) || string.IsNullOrWhiteSpace(passwordNuevo))
            {
                TempData["Error"] = "Debe completar todos los campos.";
                return View();
            }

            if (passwordNuevo != passwordConfirmar)
            {
                TempData["Error"] = "Las contraseñas nuevas no coinciden.";
                return View();
            }

            if (passwordNuevo.Length < 6)
            {
                TempData["Error"] = "La contraseña debe tener al menos 6 caracteres.";
                return View();
            }

            using (var ctx = new ColibriDbContext())
            {
                var idUsuario = (int)Session["UsuarioId"];
                var user = ctx.Usuarios.Find(idUsuario);
                if (user == null)
                    return RedirectToAction("Logout");

                var actualHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(passwordActual));
                if (user.PasswordHash != actualHash)
                {
                    TempData["Error"] = "La contraseña actual no es correcta.";
                    return View();
                }

                user.PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(passwordNuevo));
                user.CambioPasswordRequerido = false;
                user.FechaPassword = new FechasLN().ObtenerFechaActual();
                ctx.SaveChanges();

                TempData["Mensaje"] = "Contraseña cambiada correctamente.";
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index");
        }
    }
}
