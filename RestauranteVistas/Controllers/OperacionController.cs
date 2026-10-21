using AccesoADatos;
using System;
using System.Data;
using System.Data.Entity;
using System.Web.Mvc;
using System.Linq;
using Abstracciones.Models;

namespace RestauranteVistas.Controllers
{
    // One transaction covers all SaveChanges calls made by an operational action.
    public abstract class OperacionController : Controller
    {
        protected readonly ColibriDbContext db = new ColibriDbContext();
        private DbContextTransaction transaction;

        protected override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
            if (context.Result == null && string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    Guid solicitud;
                    if (!Guid.TryParse(Request.Form["requestKey"], out solicitud))
                    {
                        TempData["Error"] = "El formulario expiró. Actualice la pantalla e intente nuevamente.";
                        context.Result = RedirectToAction("Index");
                        return;
                    }
                    transaction = db.Database.BeginTransaction(IsolationLevel.Serializable);
                    string referencia = "solicitud:" + solicitud.ToString("D");
                    if (db.BitacoraAcceso.Any(s => s.ValorNuevo == referencia))
                    {
                        TempData["Mensaje"] = "Esta solicitud ya fue procesada. Consulte el estado actualizado.";
                        context.Result = RedirectToAction("Index");
                        transaction.Rollback();
                        transaction.Dispose();
                        transaction = null;
                        return;
                    }
                    db.BitacoraAcceso.Add(new BitacoraAcceso {
                        IdUsuario = (int)Session["UsuarioId"], Accion = "OPERACION",
                        ValorNuevo = referencia,
                        Detalle = context.ActionDescriptor.ControllerDescriptor.ControllerName + "/" + context.ActionDescriptor.ActionName,
                        FechaHora = new LogicaDeNegocios.General.Fechas.FechasLN().ObtenerFechaActual()
                    });
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError("Inicio de operación: {0}", ex);
                    TempData["Error"] = "No fue posible conectar con la base de datos. Intente nuevamente.";
                    context.Result = RedirectToAction("Index");
                }
            }
        }

        protected override void OnActionExecuted(ActionExecutedContext context)
        {
            try
            {
                if (transaction != null)
                {
                    if (context.Exception != null || TempData.ContainsKey("Error")) transaction.Rollback();
                    else transaction.Commit();
                }
            }
            catch (Exception ex)
            {
                context.Exception = ex;
            }
            finally
            {
                if (transaction != null) { transaction.Dispose(); transaction = null; }
            }
            if (context.Exception != null && !context.ExceptionHandled && string.Equals(Request.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Trace.TraceError("Operación revertida: {0}", context.Exception);
                TempData.Remove("Mensaje");
                TempData["Error"] = context.Exception is ArgumentException
                    ? context.Exception.Message
                    : "No se pudo guardar la operación. Consulte el estado antes de reintentar.";
                context.ExceptionHandled = true;
                context.Result = RedirectToAction("Index");
            }
            base.OnActionExecuted(context);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (transaction != null) transaction.Dispose();
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
