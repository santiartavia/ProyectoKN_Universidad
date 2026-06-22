using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class AsignarRolViewModel
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string NombreEmpleado { get; set; }
        public string RolActual { get; set; }
        public int IdRolActual { get; set; }
        public List<Rol> RolesDisponibles { get; set; }
        public int IdRolNuevo { get; set; }
        public string Mensaje { get; set; }
        public string Error { get; set; }
        public bool MostrarConfirmacion { get; set; }
        public int IdRolConfirmado { get; set; }
    }
}
