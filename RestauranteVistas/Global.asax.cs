using AccesoADatos;
using RestauranteVistas;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace RestauranteVistas
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Desactiva la inicialización/recreación automática de EF6 para Azure SQL
            Database.SetInitializer<ColibriDbContext>(null);
            ModelBinders.Binders.Add(typeof(decimal), new Models.DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal?), new Models.DecimalModelBinder());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
