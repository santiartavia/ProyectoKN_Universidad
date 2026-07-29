using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class BitacoraReportesViewModel
    {
        public List<BitacoraReporte> Registros { get; set; }
        public List<string> Acciones { get; set; }
        public int? FiltroUsuario { get; set; }
        public string FiltroAccion { get; set; }
        public string FiltroFechaInicio { get; set; }
        public string FiltroFechaFin { get; set; }
    }
}
