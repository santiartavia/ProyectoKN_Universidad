using Abstracciones.Models;
using System;
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
        List<HoraExtra> ListarPorFiltros(int? idEmpleado, DateTime? fechaInicio, DateTime? fechaFin);

        // El método del Escenario 4 que agregamos
        ResumenHorasExtraDto ObtenerResumenSemanal(int idEmpleado, DateTime fechaInicioSemana, DateTime fechaFinSemana);
    }
}