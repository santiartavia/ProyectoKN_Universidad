using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class UsuariosViewModel
    {
        public List<Usuario> Usuarios { get; set; }
        public List<Rol> Roles { get; set; }
        public Dictionary<int, string> NombresEmpleados { get; set; }
        public int UsuariosActivos { get; set; }
        public int Administradores { get; set; }
        public int RolesDefinidos { get; set; }
        public string Mensaje { get; set; }
        public string Error { get; set; }

        // Para crear usuario
        public string NuevoNombreUsuario { get; set; }
        public string NuevoCorreo { get; set; }
        public string NuevoPassword { get; set; }
        public int NuevoIdRol { get; set; }
        public string NuevoTelefono { get; set; }
        public string NuevoDireccion { get; set; }

        // Para editar
        public int EditarIdUsuario { get; set; }
        public string EditarCorreo { get; set; }
        public string EditarTelefono { get; set; }
        public string EditarDireccion { get; set; }

        // Para asignar rol
        public int AsignarIdUsuario { get; set; }
        public int AsignarIdRol { get; set; }

        // Para eliminar
        public int EliminarIdUsuario { get; set; }
        public List<int> DepurarIds { get; set; }

        // Para bitácora de acceso
        public List<BitacoraAcceso> BitacoraAcceso { get; set; }
        public string FiltroAccion { get; set; }
        public string FiltroFechaInicio { get; set; }
        public string FiltroFechaFin { get; set; }
        public string FiltroNombreUsuario { get; set; }
        public List<string> AccionesBitacoraAcceso { get; set; }
    }
}
