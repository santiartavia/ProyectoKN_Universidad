using Abstracciones.Interfaces;
using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using LogicaDeNegocios.Services;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [Filters.AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
    public class EmpleadosController : Controller
    {
        private readonly IEmpleadoService _empleadoService;
        private readonly ITurnoService _turnoService;
        private readonly IAsistenciaService _asistenciaService;
        private readonly IVacacionService _vacacionService;
        private readonly IHoraExtraService _horaExtraService;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IMesaAtendidaService _mesaAtendidaService;

        private ColibriDbContext db = new ColibriDbContext();

        public EmpleadosController()
        {
            var fechas = new FechasLN();
            var auditoria = new AuditoriaService(fechas);
            _empleadoService = new EmpleadoService(auditoria, fechas);
            _turnoService = new TurnoService(auditoria, fechas);
            _asistenciaService = new AsistenciaService(auditoria, fechas);
            _vacacionService = new VacacionService(auditoria, fechas);
            _horaExtraService = new HoraExtraService(auditoria, fechas);
            _auditoriaService = auditoria;
            _mesaAtendidaService = new MesaAtendidaService(auditoria, fechas);
        }

        public ActionResult Index(DateTime? fechaHE = null)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            DateTime fechaHorasExtra = fechaHE ?? DateTime.Today;
            ViewBag.FechaHE = fechaHorasExtra.ToString("yyyy-MM-dd");

            var modelo = new EmpleadosViewModel
            {
                Empleados = _empleadoService.ListarTodos(),
                Turnos = _turnoService.ObtenerPorFecha(DateTime.Today),
                Vacaciones = _vacacionService.ListarPendientes(),
                VacacionesAprobadas = _vacacionService.ListarPorFiltros(null, null, null, "aprobada"),
                Asistencias = _asistenciaService.ListarPorFecha(fechaHorasExtra),
                Roles = _turnoService.ObtenerRoles(),
                EmpleadosActivos = _empleadoService.ListarActivos().Count,
                TurnosProgramados = _turnoService.ObtenerPorFecha(DateTime.Today).Count,
                UltimoMovimiento = "Panel de gestión"
            };

            if (TempData["Mensaje"] != null)
                modelo.Mensaje = TempData["Mensaje"].ToString();
            if (TempData["Error"] != null)
                modelo.Error = TempData["Error"].ToString();

            return View(modelo);
        }

        [HttpPost]
        public ActionResult RegistrarEmpleado(string cedula, string nombre, string apellidos,
                                                string telefono, string correo, string rol,
                                                string fechaIngreso, decimal salarioHora)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");
            try
            {
                DateTime fi = string.IsNullOrWhiteSpace(fechaIngreso)
                    ? DateTime.Today
                    : DateTime.Parse(fechaIngreso);
                _empleadoService.Registrar(cedula, nombre, apellidos, telefono, correo,
                                           rol, salarioHora, fi, idAdmin.Value);
                TempData["Mensaje"] = "GES-001: Empleado registrado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult AdministrarEmpleado(int id)
        {
            var empleado = _empleadoService.ListarTodos().FirstOrDefault(e => e.IdEmpleado == id);
            if (empleado == null)
                return HttpNotFound("No se encontró el empleado solicitado.");

            return View(empleado);
        }

        [HttpPost]
        public ActionResult AdministrarEmpleado(int id, string telefono, string correo, string rol, decimal salarioHora)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");
            try
            {
                _empleadoService.Actualizar(id, telefono, correo, rol, salarioHora, idAdmin.Value);
                TempData["Mensaje"] = "GES-002: Empleado actualizado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult InactivarEmpleado(int id, string motivoInactivacion)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");
            try
            {
                _empleadoService.Inactivar(id, motivoInactivacion, idAdmin.Value);
                TempData["Mensaje"] = "GES-002: Empleado inactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ReactivarEmpleado(int id)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");
            try
            {
                _empleadoService.Reactivar(id, idAdmin.Value);
                TempData["Mensaje"] = "GES-002: Empleado reactivado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult CrearTurno(int idEmpleado, string fechaTurno, string horaInicio, string horaFin, string descripcion)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");
            try
            {
                var fecha = DateTime.Parse(fechaTurno);
                var inicio = TimeSpan.Parse(horaInicio);
                var fin = TimeSpan.Parse(horaFin);
                _turnoService.Crear(idEmpleado, fecha, inicio, fin, descripcion, idAdmin.Value);
                TempData["Mensaje"] = "GES-003: Turno programado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Asistencia(DateTime? fecha = null, int? mes = null, int? anio = null, int? idEmpleado = null)
        {
            DateTime fechaFiltro = fecha ?? DateTime.Today;
            ViewBag.FechaAsistencia = fechaFiltro.ToString("yyyy-MM-dd");

            var modelo = new EmpleadosViewModel
            {
                AsistenciasPendientes = _asistenciaService.ListarPendientes(),
                Asistencias = _asistenciaService.ListarPorFecha(fechaFiltro),
                UltimoMovimiento = "Control de asistencia"
            };

            int mesHist = mes ?? DateTime.Today.Month;
            int anioHist = anio ?? DateTime.Today.Year;
            ViewBag.MesHistorial = mesHist;
            ViewBag.AnioHistorial = anioHist;
            ViewBag.IdEmpleadoHistorial = idEmpleado;
            modelo.AsistenciasHistorial = _asistenciaService.ListarPorMes(mesHist, anioHist, idEmpleado);
            modelo.Empleados = _empleadoService.ListarTodos();

            return View(modelo);
        }

        [HttpPost]
        public ActionResult RegistrarEntrada(int idEmpleado)
        {
            try
            {
                _asistenciaService.RegistrarEntrada(idEmpleado);
                TempData["Mensaje"] = "Entrada registrada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Asistencia");
        }

        [HttpPost]
        public ActionResult RegistrarSalida(int idAsistencia)
        {
            try
            {
                _asistenciaService.RegistrarSalida(idAsistencia);
                TempData["Mensaje"] = "Salida registrada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Asistencia");
        }

        [HttpPost]
        public ActionResult RegistrarSalidaPorEmpleado(int idEmpleado)
        {
            try
            {
                _asistenciaService.RegistrarSalidaPorEmpleado(idEmpleado);
                TempData["Mensaje"] = "Salida registrada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Asistencia");
        }

        [HttpGet]
        public ActionResult MetricasMesas()
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            var modelo = new EmpleadosViewModel
            {
                MetricasMesas = _mesaAtendidaService.ObtenerMetricasGlobales(DateTime.Today),
                MetricasMesasSemana = _mesaAtendidaService.ObtenerMetricasGlobalesSemana(
                    DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek),
                    DateTime.Today.AddDays(6 - (int)DateTime.Today.DayOfWeek)),
                UltimoMovimiento = "Productividad - Mesas atendidas"
            };
            return View(modelo);
        }

        [HttpPost]
        public ActionResult SolicitarVacacion(int idEmpleado, string fechaInicio, string fechaFin)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");
            try
            {
                var inicio = DateTime.Parse(fechaInicio);
                var fin = DateTime.Parse(fechaFin);
                _vacacionService.Solicitar(idEmpleado, inicio, fin, idAdmin.Value);
                TempData["Mensaje"] = "GES-005: Vacaciones solicitadas correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AprobarVacacion(int idVacacion)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");
            try
            {
                _vacacionService.Aprobar(idVacacion, idAdmin.Value);
                TempData["Mensaje"] = "GES-005: Vacación aprobada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RechazarVacacion(int idVacacion, string motivoRechazo)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");
            try
            {
                _vacacionService.Rechazar(idVacacion, idAdmin.Value, motivoRechazo);
                TempData["Mensaje"] = "GES-005: Vacación rechazada.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RegistrarHorasExtra(int idAsistencia, decimal cantidadHoras)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");
            try
            {
                _horaExtraService.Registrar(idAsistencia, cantidadHoras, 1.5m, idAdmin.Value);
                TempData["Mensaje"] = "GES-006: Horas extra registradas correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AjustarHorasExtra(int idHoraExtra, decimal cantidadHoras, string motivoAjuste)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");
            try
            {
                _horaExtraService.Ajustar(idHoraExtra, cantidadHoras, motivoAjuste, idAdmin.Value);
                TempData["Mensaje"] = "GES-006: Horas extra ajustadas correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("ResumenHorasExtra");
        }

        [HttpGet]
        public ActionResult ResumenHorasExtra(int? idEmpleado, string fechaInicio, string fechaFin)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin);

            var horasExtra = _horaExtraService.ListarPorFiltros(idEmpleado, fi, ff);

            ViewBag.TotalHoras = horasExtra.Sum(h => h.CantidadHoras);
            ViewBag.TotalMonto = horasExtra.Sum(h => h.MontoCalculado);
            ViewBag.TotalRegistros = horasExtra.Count;
            ViewBag.FiltroIdEmpleado = idEmpleado;
            ViewBag.FiltroFechaInicio = fechaInicio;
            ViewBag.FiltroFechaFin = fechaFin;
            ViewBag.Empleados = _empleadoService.ListarActivos();

            if (TempData["Mensaje"] != null)
                ViewBag.Mensaje = TempData["Mensaje"].ToString();
            if (TempData["Error"] != null)
                ViewBag.Error = TempData["Error"].ToString();

            return View(horasExtra);
        }

        [HttpGet]
        public ActionResult Bitacora(int? idEmpleado, string fechaInicio, string fechaFin, string accion)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin);

            var modelo = new EmpleadosViewModel
            {
                Bitacora = _auditoriaService.Consultar(idEmpleado, null, fi, ff, accion),
                FiltroIdEmpleado = idEmpleado,
                FiltroFechaInicio = fechaInicio,
                FiltroFechaFin = fechaFin,
                FiltroAccion = accion,
                AccionesBitacora = _auditoriaService.ObtenerAccionesDistinct(),
                UltimoMovimiento = "Bitácora del sistema"
            };
            return View(modelo);
        }

        [HttpGet]
        public ActionResult ExportarBitacoraExcel(int? idEmpleado, string fechaInicio, string fechaFin, string accion)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin);

            var registros = _auditoriaService.Consultar(idEmpleado, null, fi, ff, accion);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ID,Usuario,ID Usuario,Tabla,ID Registro,Acción,Valor Anterior,Valor Nuevo,Detalle,Fecha/Hora");
            foreach (var r in registros)
            {
                sb.AppendLine($"{r.IdRegistro},{EscapeCsv(r.Usuario?.NombreUsuario)},{r.IdUsuario},{r.TablaAfectada},{r.IdRegistroAfectado},{r.Accion},{EscapeCsv(r.ValorAnterior)},{EscapeCsv(r.ValorNuevo)},{EscapeCsv(r.Detalle)},{r.FechaHora:yyyy-MM-dd HH:mm:ss}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var preamble = System.Text.Encoding.UTF8.GetPreamble();
            var withBom = new byte[preamble.Length + bytes.Length];
            preamble.CopyTo(withBom, 0);
            bytes.CopyTo(withBom, preamble.Length);

            return File(withBom, "text/csv", $"bitacora_rrhh_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        [HttpGet]
        public ActionResult BitacoraPedidos(int? idPedido, string accion, string usuarioResponsable, string fechaInicio, string fechaFin)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin).Date.AddDays(1).AddSeconds(-1);

            var consulta = db.HistorialEstadosPedido.Where(h => h.Estado == true);

            if (idPedido.HasValue)
                consulta = consulta.Where(h => h.IdPedido == idPedido.Value);

            if (!string.IsNullOrWhiteSpace(accion))
                consulta = consulta.Where(h => h.EstadoNuevo == accion || h.Detalle.Contains(accion));

            if (!string.IsNullOrWhiteSpace(usuarioResponsable))
                consulta = consulta.Where(h => h.UsuarioResponsable.Contains(usuarioResponsable));

            if (fi.HasValue)
                consulta = consulta.Where(h => h.FechaHoraCambio >= fi.Value);

            if (ff.HasValue)
                consulta = consulta.Where(h => h.FechaHoraCambio <= ff.Value);

            ViewBag.BitacoraPedidos = consulta.OrderByDescending(h => h.FechaHoraCambio).ToList();
            ViewBag.FiltroIdPedido = idPedido;
            ViewBag.FiltroAccion = accion;
            ViewBag.FiltroUsuarioResponsable = usuarioResponsable;
            ViewBag.FiltroFechaInicio = fechaInicio;
            ViewBag.FiltroFechaFin = fechaFin;

            return View();
        }

        [HttpGet]
        public ActionResult ExportarBitacoraPedidosExcel(int? idPedido, string accion, string usuarioResponsable, string fechaInicio, string fechaFin)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            DateTime? fi = null, ff = null;
            if (!string.IsNullOrWhiteSpace(fechaInicio)) fi = DateTime.Parse(fechaInicio);
            if (!string.IsNullOrWhiteSpace(fechaFin)) ff = DateTime.Parse(fechaFin).Date.AddDays(1).AddSeconds(-1);

            var consulta = db.HistorialEstadosPedido.Where(h => h.Estado == true);

            if (idPedido.HasValue)
                consulta = consulta.Where(h => h.IdPedido == idPedido.Value);

            if (!string.IsNullOrWhiteSpace(accion))
                consulta = consulta.Where(h => h.EstadoNuevo == accion || h.Detalle.Contains(accion));

            if (!string.IsNullOrWhiteSpace(usuarioResponsable))
                consulta = consulta.Where(h => h.UsuarioResponsable.Contains(usuarioResponsable));

            if (fi.HasValue)
                consulta = consulta.Where(h => h.FechaHoraCambio >= fi.Value);

            if (ff.HasValue)
                consulta = consulta.Where(h => h.FechaHoraCambio <= ff.Value);

            var registros = consulta.OrderByDescending(h => h.FechaHoraCambio).ToList();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ID,Pedido,Estado Anterior,Estado Nuevo,Responsable,Fecha/Hora,Detalle");

            foreach (var r in registros)
            {
                sb.AppendLine($"{r.IdHistorial},{r.IdPedido},{EscapeCsv(r.EstadoAnterior)},{EscapeCsv(r.EstadoNuevo)},{EscapeCsv(r.UsuarioResponsable)},{r.FechaHoraCambio:yyyy-MM-dd HH:mm:ss},{EscapeCsv(r.Detalle)}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            var preamble = System.Text.Encoding.UTF8.GetPreamble();
            var withBom = new byte[preamble.Length + bytes.Length];
            preamble.CopyTo(withBom, 0);
            bytes.CopyTo(withBom, preamble.Length);

            return File(withBom, "text/csv", $"bitacora_pedidos_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }

        [HttpPost]
        public ActionResult RegistrarAsistencia(int idEmpleado)
        {
            return RegistrarEntrada(idEmpleado);
        }

        [HttpGet]
        public ActionResult BuscarEmpleados(string termino)
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");

            var resultados = string.IsNullOrWhiteSpace(termino)
                ? _empleadoService.ListarActivos()
                : _empleadoService.Buscar(termino);

            return View("BuscarEmpleados", new EmpleadosViewModel
            {
                Empleados = resultados,
                UltimoMovimiento = string.IsNullOrWhiteSpace(termino)
                    ? "Todos los empleados activos"
                    : $"Resultados para: {termino}",
                TerminoBusqueda = termino
            });
        }

        [HttpPost]
        public ActionResult ActualizarVacacionesAcumuladas()
        {
            var idAdmin = ObtenerIdUsuarioSesion();
            if (idAdmin == null) return RedirectToAction("Index", "Login");
            try
            {
                _vacacionService.ActualizarVacacionesAcumuladas(idAdmin.Value);
                TempData["Mensaje"] = "Saldo vacacional actualizado para todos los empleados según Código de Trabajo.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        private int? ObtenerIdUsuarioSesion()
        {
            if (Session["UsuarioId"] == null) return null;
            return (int)Session["UsuarioId"];
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}