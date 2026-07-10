using System;

namespace Abstracciones.Models
{
    public class ReporteGenerado
    {
        public int IdReporte { get; set; }
        public int IdUsuario { get; set; }
        public string TipoReporte { get; set; }
        public string Parametros { get; set; }
        public string FormatoSalida { get; set; }
        public DateTime FechaGeneracion { get; set; }
        public bool Estado { get; set; }
        public Usuario Usuario { get; set; }
    }
}
