using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class BitacoraPdvViewModel
    {
        public List<RegistroPdv> Registros { get; set; }
        public Dictionary<int, string> Cajeros { get; set; }
        public List<string> AccionesDisponibles { get; set; }
        public List<Caja> CajasDisponibles { get; set; }

        public int? FiltroIdUsuario { get; set; }
        public int? FiltroIdCaja { get; set; }
        public string FiltroAccion { get; set; }
        public string FiltroFechaInicio { get; set; }
        public string FiltroFechaFin { get; set; }

        public string Mensaje { get; set; }
        public string Error { get; set; }

        public class RegistroPdv
        {
            public int IdRegistro { get; set; }
            public string NombreCaja { get; set; }
            public string NombreCajero { get; set; }
            public string AccionOperativa { get; set; }
            public int? IdVenta { get; set; }
            public string Detalle { get; set; }
            public System.DateTime FechaHora { get; set; }
        }
    }
}