using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class EditarUsuarioViewModel
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreEmpleado { get; set; }
        public string CorreoActual { get; set; }
        public string TelefonoActual { get; set; }
        public string RolActual { get; set; }
        public string Cedula { get; set; }
        public string FechaNacimiento { get; set; }
        public string Nombre { get; set; }

        public string NuevoCorreo { get; set; }
        public string NuevoTelefono { get; set; }
        public string NuevaDireccion { get; set; }
        public int? NuevoIdRol { get; set; }
        public List<Rol> RolesDisponibles { get; set; }

        public string Mensaje { get; set; }
        public string Error { get; set; }
    }
}
