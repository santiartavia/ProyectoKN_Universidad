using Abstracciones.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Abstracciones.Interfaces
{
    public interface IHoraExtraAD
    {
        HoraExtra Registrar(HoraExtra horaExtra);
        HoraExtra Ajustar(int idHoraExtra, decimal cantidadHoras, decimal montoCalculado, string motivoAjuste);
        Asistencia ObtenerAsistenciaConEmpleado(int idAsistencia);
        HoraExtra ObtenerHoraExtraConEmpleado(int idHoraExtra);
        List<HoraExtra> ObtenerRegistrosSemana(int idEmpleado, DateTime fechaInicio, DateTime fechaFin);
        List<HoraExtra> ListarPorAsistencia(int idAsistencia);
        List<HoraExtra> ListarTodas();
        List<HoraExtra> ListarPorFiltros(int? idEmpleado, DateTime? fechaInicio, DateTime? fechaFin);
    }
}