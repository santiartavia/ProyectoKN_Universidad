using System;

namespace Abstracciones.Models
{
    public class HoraExtra
    {
        public int IdHoraExtra { get; set; }
        public int IdAsistencia { get; set; }
        public decimal CantidadHoras { get; set; }
        public decimal FactorPago { get; set; }
        public decimal? MontoCalculado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string MotivoAjuste { get; set; }
        public bool Estado { get; set; }

        public Asistencia Asistencia { get; set; }
    }
}