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
    }
}
