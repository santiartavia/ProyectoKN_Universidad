using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class MisVacacionesViewModel
    {
        public Empleado Empleado { get; set; }
        public List<Vacacion> Vacaciones { get; set; }
        public decimal DiasAcumulados { get; set; }
        public decimal DiasUsados { get; set; }
        public decimal DiasDisponibles { get; set; }
        public string Mensaje { get; set; }
        public string Error { get; set; }
    }
}
