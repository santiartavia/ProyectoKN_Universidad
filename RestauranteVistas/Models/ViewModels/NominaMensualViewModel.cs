using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class NominaMensualViewModel
    {
        public List<NominaMensual> NominasPreview { get; set; }
        public List<NominaMensual> Historial { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public string Mensaje { get; set; }
        public string Error { get; set; }
    }
}
