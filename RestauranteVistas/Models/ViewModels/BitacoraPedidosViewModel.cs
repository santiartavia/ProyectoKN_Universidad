using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class BitacoraPedidosViewModel
    {
        public List<BitacoraPedido> Registros { get; set; }
        public Dictionary<int, string> Usuarios { get; set; }
        public int? FiltroIdPedido { get; set; }
        public string FiltroFechaInicio { get; set; }
        public string FiltroFechaFin { get; set; }
        public string FiltroAccion { get; set; }
        public List<string> AccionesDisponibles { get; set; }
        public int? FiltroIdUsuario { get; set; }
    }
}
