using Abstracciones.Interfaces;
using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Services;
using Newtonsoft.Json;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
    public class NominaMensualController : Controller
    {
        private readonly INominaMensualService _nominaService;
        private readonly IHoraExtraService _horaExtraService;
        private readonly IAuditoriaService _auditoriaService;

        public NominaMensualController()
        {
            var fechas = new FechasLN();
            var auditoria = new AuditoriaService(fechas);
            _nominaService = new NominaMensualService(auditoria, fechas);
            _horaExtraService = new HoraExtraService(auditoria, fechas);
            _auditoriaService = auditoria;
        }

        public ActionResult Index(int? mes, int? anio)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            int mesActual = mes ?? DateTime.Today.Month;
            int anioActual = anio ?? DateTime.Today.Year;

            var modelo = new NominaMensualViewModel
            {
                NominasPreview = _nominaService.CalcularPreview(mesActual, anioActual),
                Historial = _nominaService.ListarHistorial(mes, anio),
                Mes = mesActual,
                Anio = anioActual
            };

            if (TempData["Mensaje"] != null)
                modelo.Mensaje = TempData["Mensaje"].ToString();
            if (TempData["Error"] != null)
                modelo.Error = TempData["Error"].ToString();

            return View(modelo);
        }

        [HttpPost]
        public ActionResult Cerrar(int idEmpleado, int mes, int anio, string observaciones)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _nominaService.Cerrar(idEmpleado, mes, anio, observaciones, idAdmin.Value);
                TempData["Mensaje"] = $"Nómina cerrada correctamente para empleado #{idEmpleado}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Index", new { mes, anio });
        }

        [HttpPost]
        public ActionResult CerrarMes(int mes, int anio, string observaciones)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                _nominaService.CerrarMes(mes, anio, observaciones, idAdmin.Value);
                TempData["Mensaje"] = $"Nómina del período {mes}/{anio} cerrada para todos los empleados.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cerrar nómina: {ex.Message}";
            }

            return RedirectToAction("Index", new { mes, anio });
        }

        [HttpPost]
        public ActionResult AjustarHorasExtra(int idEmpleado, int mes, int anio, decimal cantidadHoras, string motivoAjuste)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            try
            {
                using (var ctx = new ColibriDbContext())
                {
                    var primerDia = new DateTime(anio, mes, 1);
                    var ultimoDia = primerDia.AddMonths(1).AddDays(-1);

                    var primeraAsistencia = ctx.Asistencias
                        .Where(a => a.IdEmpleado == idEmpleado && a.Estado
                            && a.FechaHoraEntrada >= primerDia
                            && a.FechaHoraEntrada <= ultimoDia
                            && a.FechaHoraSalida != null)
                        .OrderBy(a => a.FechaHoraEntrada)
                        .FirstOrDefault();

                    if (primeraAsistencia == null)
                        throw new Exception("El empleado no tiene asistencias registradas en este período.");

                    var existente = ctx.HorasExtra
                        .FirstOrDefault(h => h.IdAsistencia == primeraAsistencia.IdAsistencia && h.Estado);

                    if (existente != null)
                    {
                        existente.CantidadHoras = cantidadHoras;
                        existente.MontoCalculado = cantidadHoras *
                            ctx.Empleados.Where(e => e.IdEmpleado == idEmpleado).Select(e => e.SalarioHora).FirstOrDefault() * 1.5m;
                        existente.MotivoAjuste = motivoAjuste;
                    }
                    else
                    {
                        ctx.HorasExtra.Add(new Abstracciones.Models.HoraExtra
                        {
                            IdAsistencia = primeraAsistencia.IdAsistencia,
                            CantidadHoras = cantidadHoras,
                            FactorPago = 1.5m,
                            MontoCalculado = cantidadHoras *
                                ctx.Empleados.Where(e => e.IdEmpleado == idEmpleado).Select(e => e.SalarioHora).FirstOrDefault() * 1.5m,
                            FechaRegistro = DateTime.Today,
                            MotivoAjuste = motivoAjuste,
                            Estado = true
                        });
                    }
                    ctx.SaveChanges();

                    _auditoriaService.Registrar("Nomina_AjusteHE", idEmpleado, "UPDATE",
                        null,
                        Newtonsoft.Json.JsonConvert.SerializeObject(new { cantidadHoras, motivoAjuste }),
                        $"Ajuste de horas extra en nómina: {cantidadHoras}h para empleado #{idEmpleado} ({mes}/{anio})",
                        idAdmin.Value);
                }

                TempData["Mensaje"] = $"Horas extra ajustadas a {cantidadHoras}h para empleado #{idEmpleado}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }

            return RedirectToAction("Index", new { mes, anio });
        }

        private int? ObtenerIdUsuarioSesion()
        {
            if (Session["UsuarioId"] == null) return null;
            return (int)Session["UsuarioId"];
        }
    }
}
