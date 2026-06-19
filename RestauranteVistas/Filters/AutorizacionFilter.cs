using System.Web.Mvc;
using System.Linq;

namespace RestauranteVistas.Filters
{
    public class AutorizacionFilter : AuthorizeAttribute
    {
        public string[] RolesPermitidos { get; set; }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            if (httpContext.Session["UsuarioId"] == null)
                return false;

            if (RolesPermitidos != null && RolesPermitidos.Length > 0)
            {
                var rol = httpContext.Session["RolNombre"]?.ToString();
                return RolesPermitidos.Contains(rol);
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
                filterContext.Result = new RedirectResult("~/Home/Index");
            }
        }
    }
}