using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    public class EmpleadosController : Controller
    {
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

        private static int siguienteId = 1004;
        private static int siguienteTurnoId = 4;
        private static int siguienteMesaId = 3;

        public ActionResult Index()
        {
            EmpleadosViewModel modelo = new EmpleadosViewModel();
            modelo.Empleados = empleados;
            modelo.Turnos = turnos;
            modelo.MesasAtendidas = mesas;
            modelo.Vacaciones = vacaciones;
            modelo.HorasExtra = horasExtra;
            modelo.EmpleadosActivos = empleados.Count(e => e.Estado == "Activo");
            modelo.TurnosProgramados = turnos.Count(t => t.Estado == "Programado");
            modelo.MesasGestionadas = mesas.Count;
            modelo.UltimoMovimiento = ObtenerUltimoMovimiento();

            return View(modelo);
        }

        [HttpPost]
        public ActionResult RegistrarEmpleado(string nombre, string cedula, string rol, string telefono, string correo)
        {
            EmpleadoDemo empleado = new EmpleadoDemo();
            empleado.Id = siguienteId++;
            empleado.Nombre = nombre;
            empleado.Cedula = cedula;
            empleado.Rol = rol;
            empleado.Telefono = telefono;
            empleado.Correo = correo;
            empleado.Estado = "Activo";

            empleados.Add(empleado);

            TempData["Mensaje"] = "GES-001: Empleado registrado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AdministrarEmpleado(int id, string nombre, string rol, string telefono, string correo)
        {
            EmpleadoDemo empleado = empleados.FirstOrDefault(e => e.Id == id);

            if (empleado != null)
            {
                empleado.Nombre = nombre;
                empleado.Rol = rol;
                empleado.Telefono = telefono;
                empleado.Correo = correo;
                TempData["Mensaje"] = "GES-002: Información del empleado actualizada correctamente.";
            }

            return RedirectToAction("Index");
        }

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

            if (!TimeSpan.TryParse(horaInicio, out inicio) || !TimeSpan.TryParse(horaFin, out fin) || fin <= inicio)
            {
                TempData["Mensaje"] = "GES-003: La hora de fin debe ser posterior a la hora de inicio.";
                return RedirectToAction("Index");
            }

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

        private bool EmpleadoActivoExiste(string nombre)
        {
            return empleados.Any(e => e.Nombre == nombre && e.Estado == "Activo");
        }

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

        [HttpPost]
        public ActionResult EliminarEmpleado(int id)
        {
            EmpleadoDemo empleado = empleados.FirstOrDefault(e => e.Id == id);

            if (empleado != null)
            {
                empleado.Estado = "Inactivo";
                TempData["Mensaje"] = "GES-007: Usuario desactivado correctamente.";
            }

            return RedirectToAction("Index");
        }
    }

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
