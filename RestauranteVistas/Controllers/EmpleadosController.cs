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

        private static List<TurnoDemo> turnos = new List<TurnoDemo>();
        private static List<MesaAtendidaDemo> mesas = new List<MesaAtendidaDemo>();
        private static List<VacacionDemo> vacaciones = new List<VacacionDemo>();
        private static List<HoraExtraDemo> horasExtra = new List<HoraExtraDemo>();

        private static int siguienteId = 1004;

        public ActionResult Index()
        {
            EmpleadosViewModel modelo = new EmpleadosViewModel();
            modelo.Empleados = empleados;
            modelo.Turnos = turnos;
            modelo.MesasAtendidas = mesas;
            modelo.Vacaciones = vacaciones;
            modelo.HorasExtra = horasExtra;

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
        public ActionResult CrearTurno(string empleado, string dia, string horaInicio, string horaFin)
        {
            TurnoDemo turno = new TurnoDemo();
            turno.Empleado = empleado;
            turno.Dia = dia;
            turno.HoraInicio = horaInicio;
            turno.HoraFin = horaFin;

            turnos.Add(turno);

            TempData["Mensaje"] = "GES-003: Turno creado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult RegistrarMesa(string empleado, int numeroMesa, string fecha)
        {
            MesaAtendidaDemo mesa = new MesaAtendidaDemo();
            mesa.Empleado = empleado;
            mesa.NumeroMesa = numeroMesa;
            mesa.Fecha = fecha;

            mesas.Add(mesa);

            TempData["Mensaje"] = "GES-004: Mesa atendida registrada correctamente.";
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
        public string Empleado { get; set; }
        public string Dia { get; set; }
        public string HoraInicio { get; set; }
        public string HoraFin { get; set; }
    }

    public class MesaAtendidaDemo
    {
        public string Empleado { get; set; }
        public int NumeroMesa { get; set; }
        public string Fecha { get; set; }
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