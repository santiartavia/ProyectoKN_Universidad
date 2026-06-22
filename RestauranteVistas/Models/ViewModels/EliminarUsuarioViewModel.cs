using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class EliminarUsuarioViewModel
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreEmpleado { get; set; }
        public string Rol { get; set; }
        public string Correo { get; set; }

        public List<Usuario> UsuariosInactivos { get; set; }
        public List<int> DepurarIds { get; set; }

        public string Mensaje { get; set; }
        public string Error { get; set; }
        public bool MostrarConfirmacion { get; set; }
    }
}
