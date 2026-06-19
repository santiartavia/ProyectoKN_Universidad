using Abstracciones.Models;
using AccesoADatos;
using RestauranteVistas.Filters;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
    public class UsuariosController : Controller
    {
        public ActionResult Index()
        {
            using (var ctx = new ColibriDbContext())
            {
                var usuarios = ctx.Usuarios.Include("Rol")
                    .Where(u => u.Estado)
                    .OrderBy(u => u.IdUsuario)
                    .ToList();

                var empleados = ctx.Empleados.Where(e => e.Estado)
                    .ToDictionary(e => e.IdUsuario, e => $"{e.Nombre} {e.Apellidos}");

                var roles = ctx.Roles.Where(r => r.Estado).ToList();

                var modelo = new UsuariosViewModel
                {
                    Usuarios = usuarios,
                    Roles = roles,
                    UsuariosActivos = usuarios.Count,
                    Administradores = usuarios.Count(u => u.Rol?.NombreRol == "Administrador"),
                    RolesDefinidos = roles.Count,
                    NombresEmpleados = empleados
                };

                if (TempData["Mensaje"] != null)
                    modelo.Mensaje = TempData["Mensaje"].ToString();
                if (TempData["Error"] != null)
                    modelo.Error = TempData["Error"].ToString();

                return View(modelo);
            }
        }

        [HttpPost]
        public ActionResult CrearUsuario(string nombreUsuario, string correo, string password, int idRol)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(password) || idRol <= 0)
            {
                TempData["Error"] = "Debe completar nombre de usuario, contraseña y rol.";
                return RedirectToAction("Index");
            }

            using (var ctx = new ColibriDbContext())
            {
                var existe = ctx.Usuarios.Any(u => u.NombreUsuario == nombreUsuario && u.Estado);
                if (existe)
                {
                    TempData["Error"] = "El nombre de usuario ya existe en el sistema.";
                    return RedirectToAction("Index");
                }

                var usuario = new Usuario
                {
                    IdRol = idRol,
                    NombreUsuario = nombreUsuario,
                    Correo = correo,
                    PasswordHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password)),
                    CambioPasswordRequerido = true,
                    IntentosFallidos = 0,
                    Bloqueado = false,
                    Estado = true,
                    FechaCreacion = DateTime.UtcNow.AddHours(-6)
                };
                ctx.Usuarios.Add(usuario);
                ctx.SaveChanges();

                TempData["Mensaje"] = $"Usuario '{nombreUsuario}' creado correctamente.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult BloquearUsuario(int id)
        {
            using (var ctx = new ColibriDbContext())
            {
                var user = ctx.Usuarios.Find(id);
                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index");
                }
                user.Bloqueado = !user.Bloqueado;
                ctx.SaveChanges();
                TempData["Mensaje"] = $"Usuario {(user.Bloqueado ? "bloqueado" : "desbloqueado")} correctamente.";
            }
            return RedirectToAction("Index");
        }
    }
}
