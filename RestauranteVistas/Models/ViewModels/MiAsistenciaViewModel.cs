using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class MiAsistenciaViewModel
    {
        public Empleado Empleado { get; set; }
        public List<Asistencia> AsistenciasRecientes { get; set; }
        public Asistencia EntradaPendiente { get; set; }
        public string Mensaje { get; set; }
        public string Error { get; set; }
    }
}
