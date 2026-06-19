using Abstracciones.Interfaces;
using System;
using System.Configuration;

namespace LogicaDeNegocios.General.Fechas
{
    public class FechasLN : IFechasLN
    {
        public DateTime ObtenerFechaActual()
        {
            string zonaHorariaConfig = ConfigurationManager.AppSettings["ZonaHoraria"];
            int zonaHoraria = !string.IsNullOrEmpty(zonaHorariaConfig) ? int.Parse(zonaHorariaConfig) : 0;
            return DateTime.UtcNow.AddHours(zonaHoraria);
        }
    }
}