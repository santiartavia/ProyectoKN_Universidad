using System.Web;
using System.Web.Mvc;

namespace RestauranteVistas
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new RegistrarErrorAttribute());
            filters.Add(new ValidarModeloAttribute());
        }
    }

    public class RegistrarErrorAttribute : FilterAttribute, IExceptionFilter
    {
        private static readonly object LogLock = new object();
        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled) return;
            string referencia = System.Guid.NewGuid().ToString("N");
            try
            {
                string folder = System.Web.Hosting.HostingEnvironment.MapPath("~/App_Data");
                System.IO.Directory.CreateDirectory(folder);
                lock (LogLock)
                    System.IO.File.AppendAllText(System.IO.Path.Combine(folder, "errores.log"),
                        System.DateTime.UtcNow.ToString("O") + " " + referencia + " " + context.HttpContext.Request.HttpMethod + " " + context.HttpContext.Request.Path + System.Environment.NewLine + context.Exception + System.Environment.NewLine);
            }
            catch (System.Exception logError) { System.Diagnostics.Trace.TraceError("No se pudo registrar error: {0}; original: {1}", logError, context.Exception); }
            context.ExceptionHandled = true;
            context.HttpContext.Response.StatusCode = 500;
            context.HttpContext.Response.TrySkipIisCustomErrors = true;
            var data = new ViewDataDictionary();
            data["Referencia"] = referencia;
            context.Result = new ViewResult { ViewName = "~/Views/Shared/Error.cshtml", ViewData = data };
        }
    }

    public class ValidarModeloAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Request.HttpMethod == "POST" && !context.Controller.ViewData.ModelState.IsValid)
            {
                context.Controller.TempData["Error"] = "Revise los campos obligatorios y los formatos numéricos antes de guardar.";
                context.Result = new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary(new {
                    controller = context.ActionDescriptor.ControllerDescriptor.ControllerName, action = "Index"
                }));
            }
        }
    }
}
