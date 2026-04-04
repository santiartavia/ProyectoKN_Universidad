using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Entrar(string usuario, string rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
            {
                TempData["ErrorLogin"] = "Debe seleccionar un rol para ingresar.";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                usuario = "Usuario Demo";
            }

            Session["UsuarioDemo"] = usuario;
            Session["RolDemo"] = rol;

            switch (rol)
            {
                case "Admin":
                    return RedirectToAction("Index", "Home");

                case "Cajero":
                    return RedirectToAction("Index", "Cajero");

                case "Mesero":
                    return RedirectToAction("Index", "Mesero");

                case "Cocinero":
                    return RedirectToAction("Index", "Cocina");

                default:
                    TempData["ErrorLogin"] = "Rol inválido.";
                    return RedirectToAction("Index");
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