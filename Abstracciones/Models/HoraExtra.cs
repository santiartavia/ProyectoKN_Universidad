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

    // DTO para cumplir con el Escenario 4: Resumen Semanal
    public class ResumenHorasExtraDto
    {
        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; }
        public decimal TotalHorasExtra { get; set; }
        public decimal MontoTotalExtra { get; set; }
        public DateTime FechaInicioSemana { get; set; }
        public DateTime FechaFinSemana { get; set; }
    }
}