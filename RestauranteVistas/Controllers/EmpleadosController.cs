using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    public class EmpleadosController : Controller
    {
        // -------------------------------------------------------------------
        // MOCK DATABASE: Simulación de base de datos en memoria para el Sprint 1
        // -------------------------------------------------------------------
        private static List<EmpleadoDemo> empleados = new List<EmpleadoDemo>
        {
            new EmpleadoDemo { Id = 1001, Nombre = "María Rodríguez", Cedula = "1-1111-1111", Rol = "Mesera", Telefono = "8888-1111", Correo = "maria@colibri.com", Estado = "Activo" },
            new EmpleadoDemo { Id = 1002, Nombre = "Luis Vargas", Cedula = "2-2222-2222", Rol = "Cocinero", Telefono = "8888-2222", Correo = "luis@colibri.com", Estado = "Activo" },
            new EmpleadoDemo { Id = 1003, Nombre = "Ana Solano", Cedula = "3-3333-3333", Rol = "Cajera", Telefono = "8888-3333", Correo = "ana@colibri.com", Estado = "Activo" }
        };

        private static List<TurnoDemo> turnos = new List<TurnoDemo>
        {
            new TurnoDemo { Id = 1, Empleado = "María Rodríguez", Dia = "Lunes", HoraInicio = "11:00", HoraFin = "19:00", Area = "Salón principal", Estado = "Programado", Observaciones = "Refuerzo para almuerzo ejecutivo" },
            new TurnoDemo { Id = 2, Empleado = "Luis Vargas", Dia = "Martes", HoraInicio = "14:00", HoraFin = "22:00", Area = "Cocina caliente", Estado = "Programado", Observaciones = "Cierre de cocina" },
            new TurnoDemo { Id = 3, Empleado = "Ana Solano", Dia = "Viernes", HoraInicio = "12:00", HoraFin = "20:00", Area = "Caja", Estado = "Programado", Observaciones = "Apoyo en horas pico" }
        };

        private static List<MesaAtendidaDemo> mesas = new List<MesaAtendidaDemo>
        {
            new MesaAtendidaDemo { Id = 1, Empleado = "María Rodríguez", NumeroMesa = 4, Fecha = DateTime.Today.ToString("yyyy-MM-dd"), Franja = "Almuerzo", EstadoAtencion = "Atendida", Observaciones = "Mesa familiar, servicio completo" },
            new MesaAtendidaDemo { Id = 2, Empleado = "Ana Solano", NumeroMesa = 8, Fecha = DateTime.Today.ToString("yyyy-MM-dd"), Franja = "Cena", EstadoAtencion = "Reservada", Observaciones = "Pendiente de llegada" }
        };

        private static List<VacacionDemo> vacaciones = new List<VacacionDemo>();
        private static List<HoraExtraDemo> horasExtra = new List<HoraExtraDemo>();

        // Generadores de IDs autoincrementables simulados
        private static int siguienteId = 1004;
        private static int siguienteTurnoId = 4;
        private static int siguienteMesaId = 3;

        /// <summary>
        /// GET: /Empleados/Index
        /// Carga el panel principal de gestión compilando las métricas y los listados actuales.
        /// </summary>
        public ActionResult Index()
        {
            EmpleadosViewModel modelo = new EmpleadosViewModel();
            modelo.Empleados = empleados;
            modelo.Turnos = turnos;
            modelo.MesasAtendidas = mesas;
            modelo.Vacaciones = vacaciones;
            modelo.HorasExtra = horasExtra;

            // Cálculo de métricas para las tarjetas (Cards) del dashboard superior
            modelo.EmpleadosActivos = empleados.Count(e => e.Estado == "Activo");
            modelo.TurnosProgramados = turnos.Count(t => t.Estado == "Programado");
            modelo.MesasGestionadas = mesas.Count;
            modelo.UltimoMovimiento = ObtenerUltimoMovimiento();

            return View(modelo);
        }

        /// <summary>
        /// POST: /Empleados/RegistrarEmpleado
        /// HU: GES-001 - Registra un nuevo empleado validando duplicados.
        /// </summary>
        [HttpPost]
        public ActionResult RegistrarEmpleado(string nombre, string cedula, string rol, string telefono, string correo)
        {
            // GES-001 | Escenario 2: No se permiten empleados duplicados (Validación por Cédula)
            if (empleados.Any(e => e.Cedula == cedula))
            {
                TempData["Mensaje"] = "Error: La identificación del empleado ya existe en el sistema.";
                return RedirectToAction("Index");
            }

            // Instancia del nuevo empleado
            EmpleadoDemo empleado = new EmpleadoDemo();
            empleado.Id = siguienteId++;
            empleado.Nombre = nombre;
            empleado.Cedula = cedula;
            empleado.Rol = rol;
            empleado.Telefono = telefono;
            empleado.Correo = correo;
            empleado.Estado = "Activo"; // Por defecto, todo empleado nuevo ingresa como Activo

            empleados.Add(empleado);

            // TODO: (Sprint 2) Implementar bitácora de guardado (Escenario 7)
            TempData["Mensaje"] = "GES-001: Empleado registrado correctamente.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST: /Empleados/AdministrarEmpleado
        /// HU: GES-002 - Modifica la información de un empleado existente.
        /// </summary>
        [HttpPost]
        public ActionResult AdministrarEmpleado(int id, string nombre, string rol, string telefono, string correo, string estadoEmpleado = null)
        {
            // Búsqueda del empleado a modificar
            EmpleadoDemo empleado = empleados.FirstOrDefault(e => e.Id == id);

            if (empleado != null)
            {
                // Actualizamos los campos recibidos del formulario
                empleado.Nombre = nombre;
                empleado.Rol = rol;
                empleado.Telefono = telefono;
                empleado.Correo = correo;

                // GES-002 | Escenarios 2 y 3: Validar cambio de estado (Inactivar/Reactivar)
                if (!string.IsNullOrEmpty(estadoEmpleado))
                {
                    empleado.Estado = estadoEmpleado;
                }

                TempData["Mensaje"] = "GES-002: Información del empleado actualizada correctamente.";
            }
            else
            {
                // GES-002 | Escenario 4: Intentar editar empleado inexistente
                TempData["Mensaje"] = "Error: Empleado no encontrado.";
            }

            return RedirectToAction("Index");
        }

        // -------------------------------------------------------------------
        // MÉTODOS DE OTRAS HISTORIAS DE USUARIO (GES-003 a GES-006)
        // -------------------------------------------------------------------

        [HttpPost]
        public ActionResult CrearTurno(string empleado, string dia, string horaInicio, string horaFin, string area, string observaciones)
        {
            if (!EmpleadoActivoExiste(empleado))
            {
                TempData["Mensaje"] = "GES-003: Seleccione un empleado activo para programar el turno.";
                return RedirectToAction("Index");
            }

            TimeSpan inicio;
            TimeSpan fin;

            // Validación de coherencia de horas
            if (!TimeSpan.TryParse(horaInicio, out inicio) || !TimeSpan.TryParse(horaFin, out fin) || fin <= inicio)
            {
                TempData["Mensaje"] = "GES-003: La hora de fin debe ser posterior a la hora de inicio.";
                return RedirectToAction("Index");
            }

            // Evitar solapamiento de turnos para el mismo empleado el mismo día
            bool turnoDuplicado = turnos.Any(t =>
                t.Empleado == empleado &&
                t.Dia == dia &&
                t.Estado == "Programado");

            if (turnoDuplicado)
            {
                TempData["Mensaje"] = "GES-003: Este empleado ya tiene un turno programado para ese día.";
                return RedirectToAction("Index");
            }

            TurnoDemo turno = new TurnoDemo
            {
                Id = siguienteTurnoId++,
                Empleado = empleado,
                Dia = dia,
                HoraInicio = horaInicio,
                HoraFin = horaFin,
                Area = area,
                Estado = "Programado",
                Observaciones = string.IsNullOrWhiteSpace(observaciones) ? "Sin observaciones" : observaciones
            };

            turnos.Add(turno);
            TempData["Mensaje"] = "GES-003: Turno programado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RegistrarMesa(string empleado, int numeroMesa, string fecha, string franja, string estadoAtencion, string observaciones)
        {
            if (!EmpleadoActivoExiste(empleado))
            {
                TempData["Mensaje"] = "GES-004: Seleccione un empleado activo para registrar la mesa.";
                return RedirectToAction("Index");
            }

            if (numeroMesa <= 0)
            {
                TempData["Mensaje"] = "GES-004: El número de mesa debe ser mayor que cero.";
                return RedirectToAction("Index");
            }

            DateTime fechaMesa;
            if (!DateTime.TryParse(fecha, out fechaMesa))
            {
                TempData["Mensaje"] = "GES-004: Ingrese una fecha válida para la atención.";
                return RedirectToAction("Index");
            }

            MesaAtendidaDemo mesa = new MesaAtendidaDemo
            {
                Id = siguienteMesaId++,
                Empleado = empleado,
                NumeroMesa = numeroMesa,
                Fecha = fechaMesa.ToString("yyyy-MM-dd"),
                Franja = franja,
                EstadoAtencion = string.IsNullOrWhiteSpace(estadoAtencion) ? "Atendida" : estadoAtencion,
                Observaciones = string.IsNullOrWhiteSpace(observaciones) ? "Sin observaciones" : observaciones
            };

            mesas.Add(mesa);
            TempData["Mensaje"] = "GES-004: Atención de mesa registrada correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult GestionarVacaciones(string empleado, string fechaInicio, string fechaFin, int dias)
        {
            VacacionDemo vacacion = new VacacionDemo();
            vacacion.Empleado = empleado;
            vacacion.FechaInicio = fechaInicio;
            vacacion.FechaFin = fechaFin;
            vacacion.Dias = dias;

            vacaciones.Add(vacacion);
            TempData["Mensaje"] = "GES-005: Vacaciones registradas correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RegistrarHorasExtra(string empleado, string fecha, int cantidadHoras)
        {
            HoraExtraDemo horaExtra = new HoraExtraDemo();
            horaExtra.Empleado = empleado;
            horaExtra.Fecha = fecha;
            horaExtra.CantidadHoras = cantidadHoras;

            horasExtra.Add(horaExtra);
            TempData["Mensaje"] = "GES-006: Horas extra registradas correctamente.";
            return RedirectToAction("Index");
        }

        /// <summary>
        /// POST: /Empleados/EliminarEmpleado
        /// HU: GES-007 (y parte de GES-002) - Ejecuta un borrado lógico (inactivación)
        /// </summary>
        [HttpPost]
        public ActionResult EliminarEmpleado(int id)
        {
            EmpleadoDemo empleado = empleados.FirstOrDefault(e => e.Id == id);

            if (empleado != null)
            {
                // Soft-delete: No se elimina de la base de datos para no perder historial de turnos/mesas, solo se inactiva.
                empleado.Estado = "Inactivo";
                TempData["Mensaje"] = "GES-007: Usuario desactivado correctamente.";
            }

            return RedirectToAction("Index");
        }

        // -------------------------------------------------------------------
        // MÉTODOS AUXILIARES (HELPERS)
        // -------------------------------------------------------------------

        /// <summary>
        /// Verifica que el empleado no solo exista, sino que tenga estado "Activo" para poder asignarle tareas.
        /// </summary>
        private bool EmpleadoActivoExiste(string nombre)
        {
            return empleados.Any(e => e.Nombre == nombre && e.Estado == "Activo");
        }

        /// <summary>
        /// Obtiene el último movimiento general del restaurante para el Dashboard superior.
        /// </summary>
        private string ObtenerUltimoMovimiento()
        {
            if (mesas.Any())
            {
                MesaAtendidaDemo ultimaMesa = mesas.Last();
                return "Mesa " + ultimaMesa.NumeroMesa + " - " + ultimaMesa.Empleado;
            }

            if (turnos.Any())
            {
                TurnoDemo ultimoTurno = turnos.Last();
                return ultimoTurno.Dia + " - " + ultimoTurno.Empleado;
            }

            return "Sin registros recientes";
        }
    }

    // -------------------------------------------------------------------
    // MODELOS Y VIEWMODELS
    // -------------------------------------------------------------------

    public class EmpleadosViewModel
    {
        public List<EmpleadoDemo> Empleados { get; set; }
        public List<TurnoDemo> Turnos { get; set; }
        public List<MesaAtendidaDemo> MesasAtendidas { get; set; }
        public List<VacacionDemo> Vacaciones { get; set; }
        public List<HoraExtraDemo> HorasExtra { get; set; }
        public int EmpleadosActivos { get; set; }
        public int TurnosProgramados { get; set; }
        public int MesasGestionadas { get; set; }
        public string UltimoMovimiento { get; set; }
    }

    public class EmpleadoDemo
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Cedula { get; set; }
        public string Rol { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string Estado { get; set; }
    }

    public class TurnoDemo
    {
        public int Id { get; set; }
        public string Empleado { get; set; }
        public string Dia { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
        public string Area { get; set; }
        public string Estado { get; set; }
        public string Observaciones { get; set; }
    }

    public class MesaAtendidaDemo
    {
        public int Id { get; set; }
        public string Empleado { get; set; }
        public int NumeroMesa { get; set; }
        public string Fecha { get; set; }
        public string Franja { get; set; }
        public string EstadoAtencion { get; set; }
        public string Observaciones { get; set; }
    }

    public class VacacionDemo
    {
        public string Empleado { get; set; }
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public int Dias { get; set; }
    }

    public class HoraExtraDemo
    {
        public string Empleado { get; set; }
        public string Fecha { get; set; }
        public int CantidadHoras { get; set; }
    }
}