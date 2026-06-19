using Abstracciones.Models;
using System;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface IMesaAtendidaService
    {
        void Registrar(int idMesa, int idEmpleado, int idPedido, string origenMesa);
        List<MesaAtendida> ObtenerMetricasPorEmpleado(int idEmpleado, DateTime fecha);
        List<MesaAtendida> ObtenerMetricasPorSemana(int idEmpleado, DateTime fechaInicio, DateTime fechaFin);
        List<MesaAtendida> ObtenerMetricasGlobales(DateTime fecha);
        List<MesaAtendida> ObtenerMetricasGlobalesSemana(DateTime fechaInicio, DateTime fechaFin);
    }
}