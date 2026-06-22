using Abstracciones.Models;
using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Helpers;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    public class LoginController : Controller
    {
        private readonly FechasLN _fechas = new FechasLN();
        private const int DIAS_VIGENCIA_PASSWORD = 180;
        private const int DIAS_AVISO_PREVIO = 15;

        public ActionResult Index()
        {
            if (Session["UsuarioId"] != null)
            {
                var rol = Session["RolNombre"]?.ToString()?.ToLower();
                return RedirigirPorRol(rol);
            }

            var model = new LoginViewModel();
            if (TempData["ErrorLogin"] != null)
                model.Error = TempData["ErrorLogin"].ToString();
            if (TempData["MensajeLogin"] != null)
                model.Mensaje = TempData["MensajeLogin"].ToString();

            return View(model);
        }

        // ========== GUS 002: INICIAR SESION ==========
        [HttpPost]
        public ActionResult Entrar(string usuario, string password)
        {
            // Campos incompletos (criterio 5)
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorLogin"] = "Faltan campos para iniciar sesión.";
                return RedirectToAction("Index");
            }

            using (var ctx = new ColibriDbContext())
            {
                string ip = Request.UserHostAddress;
                string dispositivo = Request.UserAgent;

                var user = ctx.Usuarios.Include("Rol")
                    .FirstOrDefault(u => u.NombreUsuario == usuario && u.Estado);

                // Usuario no creado (criterio 2)
                if (user == null)
                {
                    TempData["ErrorLogin"] = "El usuario no existe.";
                    return RedirectToAction("Index");
                }

                // Usuario bloqueado (criterio 4)
                if (user.Bloqueado)
                {
                    TempData["ErrorLogin"] = "Se han realizado muchos intentos de autenticación, la cuenta se ha bloqueado.";
                    return RedirectToAction("Index");
                }

                // Primer ingreso / sin contraseña
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    IniciarSesion(user);
                    var empTmp = ctx.Empleados.FirstOrDefault(ee => ee.IdUsuario == user.IdUsuario && ee.Estado);
                    if (empTmp != null)
                        Session["UsuarioNombre"] = $"{empTmp.Nombre} {empTmp.Apellidos}";
                    return RedirectToAction("CrearPassword");
                }

                // Verificar contraseña con SHA-256
                bool passwordCorrecta;
                try
                {
                    passwordCorrecta = PasswordHelper.VerifyPassword(password, user.PasswordHash);
                }
                catch
                {
                    // Compatibilidad con hashes antiguos (Base64)
                    var oldHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
                    passwordCorrecta = user.PasswordHash == oldHash;

                    // Migrar a SHA-256 si usaba Base64
                    if (passwordCorrecta)
                    {
                        user.PasswordHash = PasswordHelper.HashPassword(password);
                    }
                }

                // contraseña incorrecta (criterio 3)
                if (!passwordCorrecta)
                {
                    user.IntentosFallidos++;

                    if (user.IntentosFallidos >= 5)
                    {
                        user.Bloqueado = true;
                        ctx.BitacoraAcceso.Add(new BitacoraAcceso
                        {
                            IdUsuario = user.IdUsuario,
                            Accion = "BLOQUEO",
                            Detalle = $"Cuenta bloqueada por {user.IntentosFallidos} intentos fallidos.",
                            IpOrigen = ip,
                            Dispositivo = dispositivo,
                            FechaHora = _fechas.ObtenerFechaActual()
                        });
                    }

                    if (user.IntentosFallidos >= 3)
                        user.CambioPasswordRequerido = true;

                    ctx.SaveChanges();

                    TempData["ErrorLogin"] = "contraseña incorrecta, favor intente de nuevo.";
                    return RedirectToAction("Index");
                }

                // Login exitoso - resetear intentos
                user.IntentosFallidos = 0;
                user.FechaUltimoAcceso = _fechas.ObtenerFechaActual();

                // Verificar expiración de contraseña (GUS 006)
                bool passwordExpirada = false;
                bool passwordProximaVencer = false;
                int diasRestantes = DIAS_VIGENCIA_PASSWORD;

                if (user.FechaPassword.HasValue)
                {
                    var diasTranscurridos = (int)(_fechas.ObtenerFechaActual() - user.FechaPassword.Value).TotalDays;
                    diasRestantes = Math.Max(0, DIAS_VIGENCIA_PASSWORD - diasTranscurridos);
                    passwordExpirada = diasTranscurridos >= DIAS_VIGENCIA_PASSWORD;
                    passwordProximaVencer = diasTranscurridos >= (DIAS_VIGENCIA_PASSWORD - DIAS_AVISO_PREVIO) && !passwordExpirada;

                    if (passwordExpirada)
                    {
                        user.CambioPasswordRequerido = true;
                        // Bloquear si no cambia (criterio 7)
                    }
                }

                ctx.SaveChanges();

                // Cuenta expirada (criterio 6)
                if (passwordExpirada && user.CambioPasswordRequerido)
                {
                    TempData["ErrorLogin"] = "La cuenta del usuario expiró, favor solicitar reactivación al administrador.";
                    return RedirectToAction("Index");
                }

                // Iniciar sesión
                IniciarSesion(user);
                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == user.IdUsuario && e.Estado);
                if (empleado != null)
                    Session["UsuarioNombre"] = $"{empleado.Nombre} {empleado.Apellidos}";

                // Crear sesión en BD (GUS 003, GUS 008)
                var sesion = new Sesion
                {
                    IdUsuario = user.IdUsuario,
                    EstadoSesion = "ACTIVA",
                    FechaHoraInicio = _fechas.ObtenerFechaActual(),
                    FechaHoraUltimaAct = _fechas.ObtenerFechaActual(),
                    DispositivoAcceso = dispositivo,
                    DireccionIp = ip
                };
                ctx.Sesiones.Add(sesion);
                Session["IdSesion"] = sesion.IdSesion;

                // bitácora de acceso (GUS 008)
                ctx.BitacoraAcceso.Add(new BitacoraAcceso
                {
                    IdUsuario = user.IdUsuario,
                    Accion = "LOGIN_SUCCESS",
                    Detalle = $"Inicio de sesión correcto en {_fechas.ObtenerFechaActual():dd/MM/yyyy HH:mm}",
                    IpOrigen = ip,
                    Dispositivo = dispositivo,
                    FechaHora = _fechas.ObtenerFechaActual()
                });

                ctx.SaveChanges();

                // Alerta de contraseña próxima a vencer (GUS 006)
                if (passwordProximaVencer && !user.FechaAvisoPassword.HasValue)
                {
                    user.FechaAvisoPassword = _fechas.ObtenerFechaActual();
                    ctx.SaveChanges();
                    TempData["MensajeLogin"] = $"Su contraseña está próxima a vencer. Le quedan {diasRestantes} días. Puede cambiarla desde el menú 'Cambiar contraseña'.";
                }

                // Si requiere cambio forzado de contraseña
                if (user.CambioPasswordRequerido)
                {
                    TempData["MensajeLogin"] = "No podrá continuar sin cambiar su contraseña.";
                    return RedirectToAction("CambiarPassword");
                }

                var rol = user.Rol.NombreRol.ToLower();
                return RedirigirPorRol(rol);
            }
        }

        private void IniciarSesion(Usuario user)
        {
            Session["UsuarioId"] = user.IdUsuario;
            Session["UsuarioNombre"] = user.NombreUsuario;
            Session["RolNombre"] = user.Rol.NombreRol;
            Session["RolId"] = user.IdRol;
        }

        private ActionResult RedirigirPorRol(string rol)
        {
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

        // ========== GUS 002: CREAR contraseña (primer ingreso) ==========
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

            if (!PasswordHelper.CumplePoliticaSeguridad(passwordNuevo, out string msgSeg))
            {
                TempData["Error"] = $"No se cumple con los requisitos: {msgSeg}";
                return View();
            }

            using (var ctx = new ColibriDbContext())
            {
                var idUsuario = (int)Session["UsuarioId"];
                var user = ctx.Usuarios.Include("Rol").FirstOrDefault(u => u.IdUsuario == idUsuario);
                if (user == null)
                    return RedirectToAction("Logout");

                string ip = Request.UserHostAddress;
                string dispositivo = Request.UserAgent;

                var nuevoHash = PasswordHelper.HashPassword(passwordNuevo);
                user.PasswordHash = nuevoHash;
                user.CambioPasswordRequerido = false;
                user.FechaPassword = _fechas.ObtenerFechaActual();
                user.UltimoCambioPasswordIp = ip;
                user.UltimoCambioPasswordDispositivo = dispositivo;

                // Guardar en historial
                ctx.PasswordHistorial.Add(new PasswordHistorial
                {
                    IdUsuario = user.IdUsuario,
                    PasswordHash = nuevoHash,
                    FechaCambio = _fechas.ObtenerFechaActual(),
                    MetodoCambio = "PRIMER_INGRESO",
                    Dispositivo = dispositivo,
                    DireccionIp = ip
                });

                // bitácora (GUS 008)
                ctx.BitacoraRRHH.Add(new BitacoraRRHH
                {
                    IdUsuario = user.IdUsuario,
                    TablaAfectada = "Usuarios",
                    IdRegistroAfectado = user.IdUsuario,
                    Accion = "PASSWORD_CHANGE",
                    Detalle = $"creación de contraseña (PRIMER_INGRESO) del usuario {user.NombreUsuario}",
                    IpOrigen = ip,
                    Dispositivo = dispositivo,
                    FechaHora = _fechas.ObtenerFechaActual()
                });

                ctx.SaveChanges();

                TempData["Mensaje"] = "contraseña creada correctamente.";
                var rol = user.Rol.NombreRol.ToLower();
                return RedirigirPorRol(rol);
            }
        }

        // ========== GUS 006: CAMBIAR contraseña ==========
        [HttpGet]
        public ActionResult CambiarPassword()
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Index");

            // Verificar si es por vencimiento (GUS 006 criterio 5)
            using (var ctx = new ColibriDbContext())
            {
                var idUsuario = (int)Session["UsuarioId"];
                var user = ctx.Usuarios.Find(idUsuario);
                if (user != null && user.CambioPasswordRequerido)
                {
                    ViewBag.CambioForzado = true;
                }
            }

            if (TempData["Mensaje"] != null)
                ViewBag.Mensaje = TempData["Mensaje"];
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"];

            return View();
        }

        [HttpPost]
        public ActionResult CambiarPassword(string passwordActual, string passwordNuevo, string passwordConfirmar)
        {
            if (Session["UsuarioId"] == null)
                return RedirectToAction("Index");

            var idUsuario = (int)Session["UsuarioId"];

            // Para el caso de cambio forzado, passwordActual puede ser opcional
            using (var ctx = new ColibriDbContext())
            {
                var user = ctx.Usuarios.Find(idUsuario);
                if (user == null)
                    return RedirectToAction("Logout");

                bool esCambioForzado = user.CambioPasswordRequerido || string.IsNullOrEmpty(user.PasswordHash);

                if (!esCambioForzado)
                {
                    if (string.IsNullOrWhiteSpace(passwordActual))
                    {
                        TempData["Error"] = "Debe ingresar su contraseña actual.";
                        return RedirectToAction("CambiarPassword");
                    }
                }

                if (string.IsNullOrWhiteSpace(passwordNuevo))
                {
                    TempData["Error"] = "Debe ingresar la nueva contraseña.";
                    return RedirectToAction("CambiarPassword");
                }

                if (passwordNuevo != passwordConfirmar)
                {
                    TempData["Error"] = "Las contraseñas nuevas no coinciden.";
                    return RedirectToAction("CambiarPassword");
                }

                if (!PasswordHelper.CumplePoliticaSeguridad(passwordNuevo, out string msgSeg))
                {
                    TempData["Error"] = $"Su contraseña ya no es segura y deberá ingresar una nueva: {msgSeg}";
                    return RedirectToAction("CambiarPassword");
                }

                string ip = Request.UserHostAddress;
                string dispositivo = Request.UserAgent;

                if (!esCambioForzado)
                {
                    // Verificar contraseña actual
                    bool actualCorrecta;
                    try
                    {
                        actualCorrecta = PasswordHelper.VerifyPassword(passwordActual, user.PasswordHash);
                    }
                    catch
                    {
                        var oldHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(passwordActual));
                        actualCorrecta = user.PasswordHash == oldHash;
                    }

                    if (!actualCorrecta)
                    {
                        TempData["Error"] = "La contraseña actual no es correcta.";
                        return RedirectToAction("CambiarPassword");
                    }
                }

                // Verificar que no sea igual a la anterior
                bool igualAnterior;
                try
                {
                    igualAnterior = PasswordHelper.VerifyPassword(passwordNuevo, user.PasswordHash);
                }
                catch
                {
                    var newHash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(passwordNuevo));
                    igualAnterior = user.PasswordHash == newHash;
                }

                if (igualAnterior)
                {
                    TempData["Error"] = "La contraseña debe ser diferente a la anterior.";
                    return RedirectToAction("CambiarPassword");
                }

                // Verificar historial de contraseñas (GUS 006 criterio 4)
                var historiales = ctx.PasswordHistorial
                    .Where(h => h.IdUsuario == idUsuario)
                    .OrderByDescending(h => h.FechaCambio)
                    .Take(5)
                    .ToList();

                foreach (var h in historiales)
                {
                    try
                    {
                        if (PasswordHelper.VerifyPassword(passwordNuevo, h.PasswordHash))
                        {
                            TempData["Error"] = "La contraseña debe ser diferente a las últimas 5 contraseñas utilizadas.";
                            return RedirectToAction("CambiarPassword");
                        }
                    }
                    catch { }
                }

                var nuevoHash = PasswordHelper.HashPassword(passwordNuevo);
                user.PasswordHash = nuevoHash;
                user.CambioPasswordRequerido = false;
                user.FechaPassword = _fechas.ObtenerFechaActual();
                user.FechaAvisoPassword = null;
                user.UltimoCambioPasswordIp = ip;
                user.UltimoCambioPasswordDispositivo = dispositivo;

                ctx.SaveChanges();

                // Guardar en historial
                ctx.PasswordHistorial.Add(new PasswordHistorial
                {
                    IdUsuario = user.IdUsuario,
                    PasswordHash = nuevoHash,
                    FechaCambio = _fechas.ObtenerFechaActual(),
                    MetodoCambio = esCambioForzado ? "OBLIGATORIO" : "MANUAL",
                    Dispositivo = dispositivo,
                    DireccionIp = ip
                });

                // bitácora (GUS 008)
                ctx.BitacoraRRHH.Add(new BitacoraRRHH
                {
                    IdUsuario = user.IdUsuario,
                    TablaAfectada = "Usuarios",
                    IdRegistroAfectado = user.IdUsuario,
                    Accion = "PASSWORD_CHANGE",
                    Detalle = $"Cambio de contraseña del usuario {user.NombreUsuario}",
                    IpOrigen = ip,
                    Dispositivo = dispositivo,
                    FechaHora = _fechas.ObtenerFechaActual()
                });

                ctx.SaveChanges();

                TempData["Mensaje"] = "contraseña cambiada exitosamente.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ========== GUS 003: CERRAR SESION ==========
        public ActionResult Logout()
        {
            if (Session["UsuarioId"] != null)
            {
                int idUsuario = (int)Session["UsuarioId"];
                int idSesion = Session["IdSesion"] != null ? (int)Session["IdSesion"] : 0;
                string ip = Request.UserHostAddress;
                string dispositivo = Request.UserAgent;

                using (var ctx = new ColibriDbContext())
                {
                    // Registrar cierre de sesión (GUS 003 criterio 1)
                    if (idSesion > 0)
                    {
                        var sesion = ctx.Sesiones.Find(idSesion);
                        if (sesion != null && sesion.EstadoSesion == "ACTIVA")
                        {
                            sesion.EstadoSesion = "CERRADA";
                            sesion.FechaHoraCierre = _fechas.ObtenerFechaActual();
                            sesion.MotivoCierre = "CIERRE_VOLUNTARIO";
                        }
                    }

                    // bitácora de cierre (GUS 008)
                    ctx.BitacoraAcceso.Add(new BitacoraAcceso
                    {
                        IdUsuario = idUsuario,
                        Accion = "LOGOUT",
                        Detalle = $"Cierre de sesión en {_fechas.ObtenerFechaActual():dd/MM/yyyy HH:mm}",
                        IpOrigen = ip,
                        Dispositivo = dispositivo,
                        FechaHora = _fechas.ObtenerFechaActual()
                    });

                    ctx.SaveChanges();
                }
            }

            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index");
        }

        // ========== GUS 003: VERIFICAR SESION (AJAX) ==========
        [HttpGet]
        public JsonResult VerificarSesion()
        {
            if (Session["UsuarioId"] == null)
            {
                return Json(new { activa = false, mensaje = "sesión cerrada." }, JsonRequestBehavior.AllowGet);
            }

            int idSesion = Session["IdSesion"] != null ? (int)Session["IdSesion"] : 0;

            using (var ctx = new ColibriDbContext())
            {
                if (idSesion > 0)
                {
                    var sesion = ctx.Sesiones.Find(idSesion);
                    if (sesion != null)
                    {
                        // Actualizar actividad
                        sesion.FechaHoraUltimaAct = _fechas.ObtenerFechaActual();

                        // Verificar tiempo máximo de inactividad (30 min = 1800 seg)
                        var inactividad = (_fechas.ObtenerFechaActual() - sesion.FechaHoraUltimaAct).TotalMinutes;
                        if (inactividad > 30)
                        {
                            sesion.EstadoSesion = "EXPIRADA";
                            sesion.FechaHoraCierre = _fechas.ObtenerFechaActual();
                            sesion.MotivoCierre = "INACTIVIDAD";
                            ctx.SaveChanges();

                            Session.Clear();
                            Session.Abandon();
                            return Json(new { activa = false, mensaje = "sesión expirada por inactividad. Debe iniciar sesión nuevamente." }, JsonRequestBehavior.AllowGet);
                        }

                        ctx.SaveChanges();
                    }
                }
            }

            return Json(new { activa = true }, JsonRequestBehavior.AllowGet);
        }
    }
}
