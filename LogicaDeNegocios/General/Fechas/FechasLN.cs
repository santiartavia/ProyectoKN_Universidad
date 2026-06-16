using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

