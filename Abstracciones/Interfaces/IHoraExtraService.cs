using Abstracciones.Models;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface IHoraExtraService
    {
        HoraExtra Registrar(int idAsistencia, decimal cantidadHoras, decimal factorPago, int idUsuarioAdmin);
        HoraExtra Ajustar(int idHoraExtra, decimal cantidadHoras, string motivoAjuste, int idUsuarioAdmin);
        List<HoraExtra> ListarPorAsistencia(int idAsistencia);
        decimal CalcularMontoExtra(decimal cantidadHoras, decimal salarioHora, decimal factorPago);
        List<HoraExtra> ListarTodas();
        List<HoraExtra> ListarPorFiltros(int? idEmpleado, System.DateTime? fechaInicio, System.DateTime? fechaFin);
    }
}