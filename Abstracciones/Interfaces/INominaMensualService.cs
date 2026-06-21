using Abstracciones.Models;
using System;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface INominaMensualService
    {
        List<NominaMensual> CalcularPreview(int mes, int anio);
        NominaMensual Cerrar(int idEmpleado, int mes, int anio, string observaciones, int idUsuario);
        void CerrarMes(int mes, int anio, string observaciones, int idUsuario);
        List<NominaMensual> ListarHistorial(int? mes, int? anio);
        NominaMensual ObtenerPorId(int id);
    }
}
