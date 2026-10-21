using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Filters
{
    public class AutorizacionFilter : AuthorizeAttribute
    {
        public string[] RolesPermitidos { get; set; }

        public override void OnAuthorization(AuthorizationContext context)
        {
            base.OnAuthorization(context);
            if (context.Result == null && (context.HttpContext.Session["CambioPasswordRequerido"] as bool? ?? false))
                context.Result = new RedirectResult("~/Login/CambiarPassword");
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext.Session["UsuarioId"] == null)
                return false;

            int? usuarioId = httpContext.Session["UsuarioId"] as int?;
            int? sesionId = httpContext.Session["IdSesion"] as int?;
            if (!usuarioId.HasValue || !sesionId.HasValue)
            {
                httpContext.Session.Clear();
                return false;
            }
            using (var db = new AccesoADatos.ColibriDbContext())
            {
                var sesion = db.Sesiones.FirstOrDefault(s => s.IdSesion == sesionId.Value && s.IdUsuario == usuarioId.Value);
                var usuario = db.Usuarios.Include("Rol").FirstOrDefault(u => u.IdUsuario == usuarioId.Value);
                var ahora = new LogicaDeNegocios.General.Fechas.FechasLN().ObtenerFechaActual();
                if (sesion == null || sesion.EstadoSesion != "ACTIVA" || usuario == null || !usuario.Estado ||
                    usuario.Bloqueado || usuario.Rol == null || !usuario.Rol.Estado ||
                    (ahora - sesion.FechaHoraUltimaAct).TotalMinutes >= 30)
                {
                    if (sesion != null && sesion.EstadoSesion == "ACTIVA")
                    {
                        sesion.EstadoSesion = "EXPIRADA";
                        sesion.FechaHoraCierre = ahora;
                        sesion.MotivoCierre = "INACTIVIDAD_O_REVOCACION";
                        db.SaveChanges();
                    }
                    httpContext.Session.Clear();
                    return false;
                }
                httpContext.Session["RolNombre"] = usuario.Rol.NombreRol;
                httpContext.Session["CambioPasswordRequerido"] = usuario.CambioPasswordRequerido;
                sesion.FechaHoraUltimaAct = ahora;
                db.SaveChanges();
            }

            if (RolesPermitidos != null && RolesPermitidos.Length > 0)
            {
                var rol = (httpContext.Session["RolNombre"]?.ToString() ?? "").Trim();
                return RolesPermitidos.Any(r => string.Equals(r?.Trim(), rol, StringComparison.OrdinalIgnoreCase));
            }

            return true;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["UsuarioId"] == null)
                filterContext.Result = new RedirectResult("~/Login/Index");
            else
            {
                filterContext.Controller.TempData["ErrorAutorizacion"] =
                    "Acceso denegado: permisos insuficientes";
                filterContext.Result = new HttpStatusCodeResult(403, "Acceso denegado: permisos insuficientes");
            }
        }
    }
}
