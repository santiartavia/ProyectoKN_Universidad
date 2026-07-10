using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Services;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador", "Cajero" })]
    public class ContabilidadController : Controller
    {
        private readonly IContabilidadService _contabilidadService;
        private static readonly string[] CategoriasGasto = { "Servicios públicos", "Mantenimiento", "Limpieza", "Oficina", "Transporte", "Otros" };

        public ContabilidadController()
        {
            _contabilidadService = new ContabilidadService(new FechasLN());
        }

        public ActionResult Index()
        {
            var vm = new ContabilidadViewModel
            {
                CategoriasGasto = CategoriasGasto
            };

            try
            {
                vm.Cajas = _contabilidadService.ListarCajas();
                vm.Aperturas = _contabilidadService.ListarAperturas();
                vm.Cierres = _contabilidadService.ListarCierres();
                vm.Reportes = _contabilidadService.ListarReportes();

                var primeraCaja = vm.Cajas.FirstOrDefault();
                if (primeraCaja != null)
                    vm.AperturaActiva = _contabilidadService.ObtenerAperturaActiva(primeraCaja.IdCaja);
                else
                    vm.AperturaActiva = vm.Aperturas.FirstOrDefault(a => a.Caja?.EstadoCaja == "abierta" && a.Estado);

                if (vm.AperturaActiva != null)
                {
                    using (var ctx = new ColibriDbContext())
                    {
                        vm.Ventas = ctx.Ventas.Where(v => v.IdApertura == vm.AperturaActiva.IdApertura && v.EstadoVenta == "completada" && v.Estado).ToList();
                        vm.NotasCredito = ctx.NotasCredito.Where(n => n.Venta.IdApertura == vm.AperturaActiva.IdApertura && n.Estado).ToList();
                    }
                    vm.Egresos = _contabilidadService.ListarEgresos(vm.AperturaActiva.IdApertura);
                }
                else
                {
                    vm.Ventas = new List<Venta>();
                    vm.Egresos = new List<EgresoCaja>();
                    vm.NotasCredito = new List<NotaCredito>();
                }

                var rol = Session["RolNombre"]?.ToString();
                if (rol == "Cajero" && Session["UsuarioId"] != null)
                {
                    var idUsuario = (int)Session["UsuarioId"];
                    using (var ctx = new ColibriDbContext())
                    {
                        var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == idUsuario && e.Estado);
                        if (empleado != null)
                            vm.IdCajero = empleado.IdEmpleado;
                    }
                }

                if (TempData["Mensaje"] != null) vm.Mensaje = TempData["Mensaje"].ToString();
                if (TempData["Error"] != null) vm.Error = TempData["Error"].ToString();
                if (TempData["Alerta"] != null) vm.Alerta = TempData["Alerta"].ToString();

                return View(vm);
            }
            catch (Exception ex)
            {
                vm.Error = $"Error al cargar: {ex.Message}";
                return View(vm);
            }
        }

        [HttpPost]
        public ActionResult AbrirCaja(int idCaja, int idCajero, decimal montoInicial, string observaciones)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                var apertura = _contabilidadService.AbrirCaja(idCaja, idCajero, montoInicial, observaciones, idAdmin, Request.UserHostAddress, Request.UserAgent);
                TempData["Mensaje"] = $"Caja abierta exitosamente. Apertura #{apertura.IdApertura}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al abrir caja: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CerrarCaja(int idApertura, decimal saldoReal)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            var idCajero = 0;
            if (Session["RolNombre"]?.ToString() == "Cajero" && Session["UsuarioId"] != null)
            {
                using (var ctx = new AccesoADatos.ColibriDbContext())
                {
                    var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == (int)Session["UsuarioId"] && e.Estado);
                    if (empleado != null) idCajero = empleado.IdEmpleado;
                }
            }

            if (idCajero == 0)
            {
                TempData["Error"] = "No se pudo identificar al cajero.";
                return RedirectToAction("Index");
            }

            try
            {
                var cierre = _contabilidadService.CerrarCaja(idApertura, idCajero, saldoReal, idAdmin, Request.UserHostAddress, Request.UserAgent);
                if (cierre.Descuadre)
                    TempData["Alerta"] = $"DESCUADRE detectado. Monto esperado: {cierre.SaldoEsperado:C}, monto real: {cierre.SaldoReal:C}.";
                else
                    TempData["Mensaje"] = $"Cierre #{cierre.IdCierre} registrado. Saldo final: {cierre.SaldoReal:C}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cerrar caja: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RegistrarEgreso(int idApertura, string categoriaGasto, string descripcion, decimal monto)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                _contabilidadService.RegistrarEgreso(idApertura, idAdmin, categoriaGasto, descripcion, monto, Request.UserHostAddress, Request.UserAgent);
                TempData["Mensaje"] = $"Egreso registrado: {categoriaGasto} - {monto:C}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al registrar egreso: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AnularVenta(int idVenta, string motivo)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                _contabilidadService.AnularVenta(idVenta, idAdmin, motivo, Request.UserHostAddress, Request.UserAgent);
                TempData["Mensaje"] = $"Venta #{idVenta} anulada exitosamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al anular venta: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult GenerarReporte(string tipoReporte, string formato, string parametros)
        {
            var idAdmin = ObtenerIdAdmin();
            if (idAdmin == 0) return RedirectToAction("Index", "Login");

            try
            {
                _contabilidadService.GenerarReporte(idAdmin, tipoReporte, formato, parametros);
                TempData["Mensaje"] = $"Reporte '{tipoReporte}' generado en formato {formato}.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al generar reporte: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        private int ObtenerIdAdmin()
        {
            return Session["UsuarioId"] != null ? (int)Session["UsuarioId"] : 0;
        }
    }
}
