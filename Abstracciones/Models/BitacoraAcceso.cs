using System;

namespace Abstracciones.Models
{
    public class BitacoraAcceso
    {
        public int IdRegistro { get; set; }
        public int IdUsuario { get; set; }
        public string Accion { get; set; }
        public string ValorAnterior { get; set; }
        public string ValorNuevo { get; set; }
        public string Detalle { get; set; }
        public string IpOrigen { get; set; }
        public string Dispositivo { get; set; }
        public DateTime FechaHora { get; set; }

        public Usuario Usuario { get; set; }
    }
}
