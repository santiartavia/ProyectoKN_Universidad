using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Helpers;
using LogicaDeNegocios.Services;
using RestauranteVistas.Filters;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
    public class UsuariosController : Controller
    {
        private readonly IFechasLN _fechas = new FechasLN();
        private readonly IUsuarioService _usuarioService = new UsuarioService(new FechasLN());

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

        // ========== GUS 001: AGREGAR USUARIO ==========
        [HttpPost]
        public ActionResult CrearUsuario(UsuariosViewModel model)
        {
            bool todosVacios = string.IsNullOrWhiteSpace(model.NuevoNombreUsuario) &&
                               string.IsNullOrWhiteSpace(model.NuevoCorreo) &&
                               string.IsNullOrWhiteSpace(model.NuevoPassword) &&
                               model.NuevoIdRol <= 0;

            if (todosVacios)
            {
                TempData["Error"] = "Los campos están en blanco.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(model.NuevoNombreUsuario) ||
                string.IsNullOrWhiteSpace(model.NuevoCorreo) ||
                string.IsNullOrWhiteSpace(model.NuevoPassword) ||
                model.NuevoIdRol <= 0)
            {
                TempData["Error"] = "Faltan campos requeridos.";
                return RedirectToAction("Index");
            }

            if (PasswordHelper.ContieneCaracteresInvalidos(model.NuevoNombreUsuario, out string msgNombre))
            {
                TempData["Error"] = $"El atributo nombre de usuario no corresponde al tipo de dato: {msgNombre}";
                return RedirectToAction("Index");
            }

            if (PasswordHelper.ContieneCaracteresInvalidos(model.NuevoCorreo, out string msgCorreo))
            {
                TempData["Error"] = $"El atributo correo no corresponde al tipo de dato: {msgCorreo}";
                return RedirectToAction("Index");
            }

            if (!PasswordHelper.EsFormatoCorreoValido(model.NuevoCorreo))
            {
                TempData["Error"] = "El formato del correo no es válido.";
                return RedirectToAction("Index");
            }

            if (!PasswordHelper.CumplePoliticaSeguridad(model.NuevoPassword, out string msgPassword))
            {
                TempData["Error"] = $"No se cumple con los caracteres o tamaño requerido para la contraseña: {msgPassword}";
                return RedirectToAction("Index");
            }

            using (var ctx = new ColibriDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                if (ctx.Usuarios.Any(u => u.NombreUsuario == model.NuevoNombreUsuario && u.Estado))
                {
                    TempData["Error"] = "El usuario ya existe.";
                    return RedirectToAction("Index");
                }

                if (ctx.Usuarios.Any(u => u.Correo == model.NuevoCorreo && u.Estado))
                {
                    TempData["Error"] = "El correo ya está registrado.";
                    return RedirectToAction("Index");
                }

                var rol = ctx.Roles.Find(model.NuevoIdRol);
                if (rol == null || !rol.Estado)
                {
                    TempData["Error"] = "El rol especificado no existe.";
                    return RedirectToAction("Index");
                }

                string ip = Request.UserHostAddress;
                string dispositivo = Request.UserAgent;

                var passwordHash = PasswordHelper.HashPassword(model.NuevoPassword);
                var usuario = new Usuario
                {
                    IdRol = model.NuevoIdRol,
                    NombreUsuario = model.NuevoNombreUsuario,
                    Correo = model.NuevoCorreo,
                    PasswordHash = passwordHash,
                    CambioPasswordRequerido = false,
                    IntentosFallidos = 0,
                    Bloqueado = false,
                    Estado = true,
                    FechaCreacion = _fechas.ObtenerFechaActual(),
                    FechaPassword = _fechas.ObtenerFechaActual()
                };
                ctx.Usuarios.Add(usuario);
                ctx.SaveChanges();

                // Guardar en historial de contraseñas
                ctx.PasswordHistorial.Add(new PasswordHistorial
                {
                    IdUsuario = usuario.IdUsuario,
                    PasswordHash = passwordHash,
                    FechaCambio = _fechas.ObtenerFechaActual(),
                    MetodoCambio = "MANUAL",
                    Dispositivo = dispositivo,
                    DireccionIp = ip
                });

                // bitácora GUS 008
                ctx.BitacoraRRHH.Add(new BitacoraRRHH
                {
                    IdUsuario = GetAdminId(),
                    TablaAfectada = "Usuarios",
                    IdRegistroAfectado = usuario.IdUsuario,
                    Accion = "INSERT",
                    ValorNuevo = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        usuario.NombreUsuario, usuario.Correo,
                        Rol = rol.NombreRol, usuario.Estado,
                        FechaCreacion = usuario.FechaCreacion
                    }),
                    Detalle = $"Se agregó un nuevo usuario: {model.NuevoNombreUsuario} con rol {rol.NombreRol}",
                    IpOrigen = ip,
                    Dispositivo = dispositivo,
                    FechaHora = _fechas.ObtenerFechaActual()
                });

                ctx.SaveChanges();
                tx.Commit();

                TempData["Mensaje"] = "Usuario agregado de manera exitosa.";
            }

            return RedirectToAction("Index");
        }

        // ========== GUS 004: EDITAR USUARIO ==========
        [HttpGet]
        public ActionResult Editar(int id)
        {
            using (var ctx = new ColibriDbContext())
            {
                var usuario = ctx.Usuarios.Include("Rol")
                    .FirstOrDefault(u => u.IdUsuario == id && u.Estado);

                if (usuario == null)
                {
                    TempData["Error"] = "El usuario con ese número de ID no existe, que vuelva a poner el número correctamente.";
                    return RedirectToAction("Index");
                }

                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == id && e.Estado);
                var roles = ctx.Roles.Where(r => r.Estado).ToList();

                var modelo = new EditarUsuarioViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    NombreUsuario = usuario.NombreUsuario,
                    NombreEmpleado = empleado != null ? $"{empleado.Nombre} {empleado.Apellidos}" : "",
                    CorreoActual = usuario.Correo,
                    TelefonoActual = empleado?.Telefono ?? "",
                    RolActual = usuario.Rol?.NombreRol ?? "",
                    Cedula = empleado?.Cedula ?? "",
                    RolesDisponibles = roles,
                    NuevoCorreo = usuario.Correo,
                    NuevoTelefono = empleado?.Telefono ?? ""
                };

                if (TempData["Mensaje"] != null)
                    modelo.Mensaje = TempData["Mensaje"].ToString();
                if (TempData["Error"] != null)
                    modelo.Error = TempData["Error"].ToString();

                return View(modelo);
            }
        }

        [HttpPost]
        public ActionResult Editar(EditarUsuarioViewModel model)
        {
            if (model.IdUsuario <= 0)
            {
                TempData["Error"] = "ID de usuario inválido.";
                return RedirectToAction("Index");
            }

            string ip = Request.UserHostAddress;
            string dispositivo = Request.UserAgent;
            var idAdmin = GetAdminId();

            try
            {
                // Combinar provincia + cantón + detalle en una sola dirección
                var provincia = model.NuevaProvincia?.Trim();
                var canton = model.NuevoCanton?.Trim();
                var detalle = model.NuevaDireccionDetalle?.Trim();
                model.NuevaDireccion = (!string.IsNullOrWhiteSpace(provincia) && !string.IsNullOrWhiteSpace(canton))
                    ? (string.IsNullOrWhiteSpace(detalle) ? $"{provincia}, {canton}" : $"{provincia}, {canton}, {detalle}")
                    : null;

                _usuarioService.EditarUsuario(
                    model.IdUsuario,
                    model.NuevoCorreo,
                    model.NuevoTelefono,
                    model.NuevaDireccion,
                    idAdmin,
                    ip,
                    dispositivo);

                TempData["Mensaje"] = "Información actualizada exitosamente.";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                model.Error = ex.Message;
                return CargarEditarViewModel(model);
            }
            catch (InvalidOperationException ex)
            {
                model.Error = ex.Message;
                return CargarEditarViewModel(model);
            }
            catch (Exception ex)
            {
                model.Error = $"Error al editar: {ex.Message}. Vuelva a intentarlo ingresando los datos de nuevo.";
                return CargarEditarViewModel(model);
            }
        }

        private ActionResult CargarEditarViewModel(EditarUsuarioViewModel model)
        {
            using (var ctx = new ColibriDbContext())
            {
                var usuario = ctx.Usuarios.Include("Rol").FirstOrDefault(u => u.IdUsuario == model.IdUsuario);
                if (usuario != null)
                {
                    var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == model.IdUsuario);
                    model.NombreUsuario = usuario.NombreUsuario;
                    model.NombreEmpleado = empleado != null ? $"{empleado.Nombre} {empleado.Apellidos}" : "";
                    model.CorreoActual = usuario.Correo;
                    model.RolActual = usuario.Rol?.NombreRol ?? "";
                    model.Cedula = empleado?.Cedula ?? "";
                    model.RolesDisponibles = ctx.Roles.Where(r => r.Estado).ToList();
                }
            }

            if (TempData["Mensaje"] != null)
                model.Mensaje = TempData["Mensaje"].ToString();
            if (string.IsNullOrEmpty(model.Error) && TempData["Error"] != null)
                model.Error = TempData["Error"].ToString();

            return View("Editar", model);
        }

        // ========== GUS 005: ASIGNAR ROL ==========
        [HttpGet]
        public ActionResult AsignarRol(int id)
        {
            using (var ctx = new ColibriDbContext())
            {
                var usuario = ctx.Usuarios.Include("Rol")
                    .FirstOrDefault(u => u.IdUsuario == id && u.Estado);

                if (usuario == null)
                {
                    TempData["Error"] = "El usuario no existe.";
                    return RedirectToAction("Index");
                }

                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == id && e.Estado);
                var roles = ctx.Roles.Where(r => r.Estado).ToList();

                var modelo = new AsignarRolViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    NombreUsuario = usuario.NombreUsuario,
                    NombreEmpleado = empleado != null ? $"{empleado.Nombre} {empleado.Apellidos}" : "",
                    RolActual = usuario.Rol?.NombreRol ?? "",
                    IdRolActual = usuario.IdRol,
                    RolesDisponibles = roles
                };

                if (TempData["Mensaje"] != null)
                    modelo.Mensaje = TempData["Mensaje"].ToString();
                if (TempData["Error"] != null)
                    modelo.Error = TempData["Error"].ToString();

                return View(modelo);
            }
        }

        [HttpPost]
        public ActionResult AsignarRol(AsignarRolViewModel model)
        {
            if (model.MostrarConfirmacion)
            {
                using (var ctx = new ColibriDbContext())
                {
                    var usuario = ctx.Usuarios.Include("Rol")
                        .FirstOrDefault(u => u.IdUsuario == model.IdUsuario && u.Estado);
                    if (usuario == null)
                    {
                        TempData["Error"] = "El usuario no existe.";
                        return RedirectToAction("Index");
                    }

                    var rolNuevo = ctx.Roles.Find(model.IdRolConfirmado);
                    if (rolNuevo == null || !rolNuevo.Estado)
                    {
                        TempData["Error"] = "Rol inválido porque no está definido.";
                        return RedirectToAction("Index");
                    }

                    if (usuario.IdRol == model.IdRolConfirmado)
                    {
                        TempData["Error"] = "El usuario ya tiene ese rol asignado.";
                        return RedirectToAction("AsignarRol", new { id = model.IdUsuario });
                    }

                    string ip = Request.UserHostAddress;
                    string dispositivo = Request.UserAgent;
                    string rolAnterior = usuario.Rol?.NombreRol ?? "";

                    usuario.IdRol = model.IdRolConfirmado;

                    ctx.BitacoraRRHH.Add(new BitacoraRRHH
                    {
                        IdUsuario = GetAdminId(),
                        TablaAfectada = "Usuarios",
                        IdRegistroAfectado = usuario.IdUsuario,
                        Accion = "ROLE_ASSIGNMENT",
                        ValorAnterior = $"Rol anterior: {rolAnterior}",
                        ValorNuevo = $"Rol nuevo: {rolNuevo.NombreRol}",
                        Detalle = $"Se asignó rol {rolNuevo.NombreRol} al usuario {usuario.NombreUsuario} en fecha {_fechas.ObtenerFechaActual():dd/MM/yyyy}",
                        IpOrigen = ip,
                        Dispositivo = dispositivo,
                        FechaHora = _fechas.ObtenerFechaActual()
                    });

                    ctx.SaveChanges();

                    TempData["Mensaje"] = $"Rol asignado correctamente al usuario.";
                    return RedirectToAction("Index");
                }
            }

            // Primera vez: mostrar confirmación
            if (model.IdRolNuevo <= 0)
            {
                TempData["Error"] = "Debe seleccionar un rol.";
                return RedirectToAction("AsignarRol", new { id = model.IdUsuario });
            }

            using (var ctx = new ColibriDbContext())
            {
                var usuario = ctx.Usuarios.Include("Rol")
                    .FirstOrDefault(u => u.IdUsuario == model.IdUsuario && u.Estado);
                if (usuario == null)
                {
                    TempData["Error"] = "El usuario no existe.";
                    return RedirectToAction("Index");
                }

                if (!usuario.Estado)
                {
                    TempData["Error"] = "El usuario no tiene permisos (cuenta inactiva).";
                    return RedirectToAction("AsignarRol", new { id = model.IdUsuario });
                }

                var rolNuevo = ctx.Roles.Find(model.IdRolNuevo);
                if (rolNuevo == null || !rolNuevo.Estado)
                {
                    TempData["Error"] = "Rol inválido porque no está definido.";
                    return RedirectToAction("AsignarRol", new { id = model.IdUsuario });
                }

                if (usuario.IdRol == model.IdRolNuevo)
                {
                    TempData["Error"] = "El usuario ya tiene ese rol asignado.";
                    return RedirectToAction("AsignarRol", new { id = model.IdUsuario });
                }

                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == model.IdUsuario && e.Estado);
                var roles = ctx.Roles.Where(r => r.Estado).ToList();

                var confirmacion = new AsignarRolViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    NombreUsuario = usuario.NombreUsuario,
                    NombreEmpleado = empleado != null ? $"{empleado.Nombre} {empleado.Apellidos}" : "",
                    RolActual = usuario.Rol?.NombreRol ?? "",
                    IdRolActual = usuario.IdRol,
                    RolesDisponibles = roles,
                    MostrarConfirmacion = true,
                    IdRolConfirmado = model.IdRolNuevo,
                    Mensaje = $"¿Es el rol {rolNuevo.NombreRol} correcto para {usuario.NombreUsuario}?"
                };

                return View(confirmacion);
            }
        }

        // ========== GUS 007: ELIMINAR USUARIO ==========
        [HttpGet]
        public ActionResult Eliminar(int id)
        {
            using (var ctx = new ColibriDbContext())
            {
                var usuario = ctx.Usuarios.Include("Rol")
                    .FirstOrDefault(u => u.IdUsuario == id);

                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no existente.";
                    return RedirectToAction("Index");
                }

                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == id && e.Estado);
                var inactivos = ctx.Usuarios.Include("Rol")
                    .Where(u => !u.Estado).OrderBy(u => u.IdUsuario).ToList();

                var modelo = new EliminarUsuarioViewModel
                {
                    IdUsuario = usuario.IdUsuario,
                    NombreUsuario = usuario.NombreUsuario,
                    NombreEmpleado = empleado != null ? $"{empleado.Nombre} {empleado.Apellidos}" : "",
                    Rol = usuario.Rol?.NombreRol ?? "",
                    Correo = usuario.Correo,
                    UsuariosInactivos = inactivos,
                    MostrarConfirmacion = false
                };

                return View(modelo);
            }
        }

        [HttpPost]
        public ActionResult Eliminar(EliminarUsuarioViewModel model)
        {
            if (model.IdUsuario <= 0)
            {
                TempData["Error"] = "ID de usuario inválido.";
                return RedirectToAction("Index");
            }

            if (!model.MostrarConfirmacion)
            {
                // Mostrar confirmación
                return RedirectToAction("Eliminar", new { id = model.IdUsuario });
            }

            // Confirmado: proceder con eliminación
            using (var ctx = new ColibriDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                var usuario = ctx.Usuarios.Include("Rol")
                    .FirstOrDefault(u => u.IdUsuario == model.IdUsuario);

                if (usuario == null)
                {
                    TempData["Error"] = "Usuario no existente.";
                    return RedirectToAction("Index");
                }

                // No eliminar único administrador
                if (usuario.Rol?.NombreRol == "Administrador" && usuario.Estado)
                {
                    var adminCount = ctx.Usuarios.Count(u => u.Rol.NombreRol == "Administrador" && u.Estado);
                    if (adminCount <= 1)
                    {
                        TempData["Error"] = "El usuario no puede ser eliminado: es el único administrador del sistema.";
                        return RedirectToAction("Index");
                    }
                }

                string ip = Request.UserHostAddress;
                string dispositivo = Request.UserAgent;
                string nombreUsuario = usuario.NombreUsuario;
                string rolUsuario = usuario.Rol?.NombreRol ?? "";

                usuario.Estado = false;
                usuario.Bloqueado = true;

                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == model.IdUsuario && e.Estado);
                if (empleado != null)
                {
                    empleado.Estado = false;
                    empleado.MotivoInactivacion = "Usuario eliminado por administrador";
                    empleado.FechaModificacion = _fechas.ObtenerFechaActual();
                }

                ctx.BitacoraRRHH.Add(new BitacoraRRHH
                {
                    IdUsuario = GetAdminId(),
                    TablaAfectada = "Usuarios",
                    IdRegistroAfectado = usuario.IdUsuario,
                    Accion = "DELETE",
                    ValorAnterior = Newtonsoft.Json.JsonConvert.SerializeObject(new { Nombre = nombreUsuario, Rol = rolUsuario, Estado = true }),
                    ValorNuevo = Newtonsoft.Json.JsonConvert.SerializeObject(new { Estado = false, Bloqueado = true }),
                    Detalle = $"Usuario eliminado: {nombreUsuario} (Rol: {rolUsuario}) en {_fechas.ObtenerFechaActual():dd/MM/yyyy HH:mm}",
                    IpOrigen = ip,
                    Dispositivo = dispositivo,
                    FechaHora = _fechas.ObtenerFechaActual()
                });

                ctx.SaveChanges();
                tx.Commit();

                TempData["Mensaje"] = $"Usuario {nombreUsuario} ha sido eliminado exitosamente.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CancelarEliminacion(int id)
        {
            TempData["Mensaje"] = "Eliminación cancelada.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DepurarUsuarios(List<int> idsDepurar)
        {
            if (idsDepurar == null || idsDepurar.Count == 0)
            {
                TempData["Error"] = "Seleccione al menos un usuario para depurar.";
                return RedirectToAction("Index");
            }

            using (var ctx = new ColibriDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                string ip = Request.UserHostAddress;
                string dispositivo = Request.UserAgent;

                var usuarios = ctx.Usuarios.Where(u => idsDepurar.Contains(u.IdUsuario) && !u.Estado).ToList();

                foreach (var u in usuarios)
                {
                    ctx.BitacoraRRHH.Add(new BitacoraRRHH
                    {
                        IdUsuario = GetAdminId(),
                        TablaAfectada = "Usuarios",
                        IdRegistroAfectado = u.IdUsuario,
                        Accion = "DELETE",
                        ValorAnterior = Newtonsoft.Json.JsonConvert.SerializeObject(new { u.NombreUsuario, u.Correo }),
                        ValorNuevo = "Eliminado permanentemente",
                        Detalle = $"depuración: usuario {u.NombreUsuario} eliminado permanentemente.",
                        IpOrigen = ip,
                        Dispositivo = dispositivo,
                        FechaHora = _fechas.ObtenerFechaActual()
                    });
                    ctx.Usuarios.Remove(u);
                }

                ctx.SaveChanges();
                tx.Commit();

                TempData["Mensaje"] = $"{usuarios.Count} usuarios eliminados permanentemente.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ResetearPassword(int id)
        {
            using (var ctx = new ColibriDbContext())
            {
                var user = ctx.Usuarios.Include("Rol").FirstOrDefault(u => u.IdUsuario == id && u.Estado);
                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index");
                }

                string ip = Request.UserHostAddress;
                string dispositivo = Request.UserAgent;

                user.PasswordHash = "";
                user.CambioPasswordRequerido = true;

                ctx.BitacoraRRHH.Add(new BitacoraRRHH
                {
                    IdUsuario = GetAdminId(),
                    TablaAfectada = "Usuarios",
                    IdRegistroAfectado = user.IdUsuario,
                    Accion = "PASSWORD_CHANGE",
                    Detalle = $"Administrador restablecio la contrasena de {user.NombreUsuario}. Debera crear una nueva al iniciar sesion.",
                    IpOrigen = ip,
                    Dispositivo = dispositivo,
                    FechaHora = _fechas.ObtenerFechaActual()
                });

                ctx.SaveChanges();

                TempData["Mensaje"] = $"Contrasena restablecida. El usuario {user.NombreUsuario} debera crear una nueva al iniciar sesion.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult BloquearUsuario(int id)
        {
            using (var ctx = new ColibriDbContext())
            {
                var user = ctx.Usuarios.Include("Rol").FirstOrDefault(u => u.IdUsuario == id);
                if (user == null)
                {
                    TempData["Error"] = "Usuario no encontrado.";
                    return RedirectToAction("Index");
                }

                user.Bloqueado = !user.Bloqueado;

                ctx.BitacoraRRHH.Add(new BitacoraRRHH
                {
                    IdUsuario = GetAdminId(),
                    TablaAfectada = "Usuarios",
                    IdRegistroAfectado = user.IdUsuario,
                    Accion = user.Bloqueado ? "DESACTIVACION" : "ACTIVACION",
                    Detalle = $"Usuario {(user.Bloqueado ? "bloqueado" : "desbloqueado")}: {user.NombreUsuario}",
                    FechaHora = _fechas.ObtenerFechaActual()
                });

                ctx.SaveChanges();
                TempData["Mensaje"] = $"Usuario {(user.Bloqueado ? "bloqueado" : "desbloqueado")} correctamente.";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult BitacoraAcceso(string nombreUsuario, string fechaInicio, string fechaFin, string accion)
        {
            var idAdmin = GetAdminId();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin);

            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.BitacoraAcceso.Include("Usuario").AsQueryable();

                if (!string.IsNullOrWhiteSpace(nombreUsuario))
                    query = query.Where(b => b.Usuario.NombreUsuario.Contains(nombreUsuario));
                if (fi.HasValue)
                    query = query.Where(b => b.FechaHora >= fi.Value);
                if (ff.HasValue)
                    query = query.Where(b => b.FechaHora <= ff.Value.AddDays(1));
                if (!string.IsNullOrWhiteSpace(accion))
                    query = query.Where(b => b.Accion == accion);

                var registros = query.OrderByDescending(b => b.FechaHora).ToList();

                var acciones = ctx.BitacoraAcceso
                    .Select(b => b.Accion)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList();

                var modelo = new UsuariosViewModel
                {
                    BitacoraAcceso = registros,
                    FiltroNombreUsuario = nombreUsuario,
                    FiltroFechaInicio = fechaInicio,
                    FiltroFechaFin = fechaFin,
                    FiltroAccion = accion,
                    AccionesBitacoraAcceso = acciones
                };

                return View(modelo);
            }
        }

        [HttpGet]
        public ActionResult ExportarBitacoraAccesoExcel(string nombreUsuario, string fechaInicio, string fechaFin, string accion)
        {
            var idAdmin = GetAdminId();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin);

            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.BitacoraAcceso.Include("Usuario").AsQueryable();

                if (!string.IsNullOrWhiteSpace(nombreUsuario))
                    query = query.Where(b => b.Usuario.NombreUsuario.Contains(nombreUsuario));
                if (fi.HasValue)
                    query = query.Where(b => b.FechaHora >= fi.Value);
                if (ff.HasValue)
                    query = query.Where(b => b.FechaHora <= ff.Value.AddDays(1));
                if (!string.IsNullOrWhiteSpace(accion))
                    query = query.Where(b => b.Accion == accion);

                var registros = query.OrderByDescending(b => b.FechaHora).ToList();

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("ID,Usuario,ID Usuario,Acción,Detalle,Ip Origen,Dispositivo,Fecha/Hora");
                foreach (var r in registros)
                {
                    sb.AppendLine($"{r.IdRegistro},{EscapeCsv(r.Usuario?.NombreUsuario)},{r.IdUsuario},{r.Accion},{EscapeCsv(r.Detalle)},{EscapeCsv(r.IpOrigen)},{EscapeCsv(r.Dispositivo)},{r.FechaHora:yyyy-MM-dd HH:mm:ss}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                var preamble = System.Text.Encoding.UTF8.GetPreamble();
                var withBom = new byte[preamble.Length + bytes.Length];
                preamble.CopyTo(withBom, 0);
                bytes.CopyTo(withBom, preamble.Length);

                return File(withBom, "text/csv", $"bitacora_acceso_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        // Obtener datos completos del empleado asociado a un usuario (JSON)
        [HttpGet]
        public JsonResult ObtenerEmpleado(int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                var usuario = ctx.Usuarios.Include("Rol").FirstOrDefault(u => u.IdUsuario == idUsuario && u.Estado);
                if (usuario == null)
                    return Json(new { error = "Usuario no encontrado." }, JsonRequestBehavior.AllowGet);

                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == idUsuario);
                if (empleado == null)
                    return Json(new { error = "El usuario no tiene un empleado asociado." }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    idEmpleado = empleado.IdEmpleado,
                    cedula = empleado.Cedula,
                    nombre = empleado.Nombre,
                    apellidos = empleado.Apellidos,
                    telefono = empleado.Telefono,
                    correo = usuario.Correo,
                    direccion = usuario.Direccion ?? empleado.Direccion ?? "",
                    nombreUsuario = usuario.NombreUsuario,
                    rol = usuario.Rol?.NombreRol ?? "",
                    salarioHora = empleado.SalarioHora.ToString("N2"),
                    fechaIngreso = empleado.FechaIngreso.ToString("dd/MM/yyyy"),
                    fechaModificacion = empleado.FechaModificacion?.ToString("dd/MM/yyyy HH:mm") ?? "—",
                    vacacionesDisponibles = empleado.DiasVacacionesDisponibles.ToString("F1"),
                    estado = empleado.Estado ? "Activo" : "Inactivo"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private int GetAdminId()
        {
            return Session["UsuarioId"] != null ? (int)Session["UsuarioId"] : 0;
        }
    }
}
