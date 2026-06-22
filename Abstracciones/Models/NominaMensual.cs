using System;

namespace Abstracciones.Models
{
    public class NominaMensual
    {
        public int IdNominaMensual { get; set; }
        public int IdEmpleado { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public decimal HorasTrabajadas { get; set; }
        public decimal HorasExtra { get; set; }
        public decimal VacacionesPagadas { get; set; }
        public decimal HorasPorDiaVacacion { get; set; }
        public decimal MontoVacaciones { get; set; }
        public int DiasTrabajados { get; set; }
        public int DiasAusentes { get; set; }
        public decimal ValorHora { get; set; }
        public decimal SalarioBase { get; set; }
        public decimal MontoHorasExtra { get; set; }
        public decimal Bonificaciones { get; set; }
        public decimal SalarioBruto { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaCierre { get; set; }
        public string Observaciones { get; set; }
        public int CreadoPor { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Empleado Empleado { get; set; }
    }
}
