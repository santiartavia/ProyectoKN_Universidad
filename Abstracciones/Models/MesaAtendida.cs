namespace Abstracciones.Models
{
    public class MesaAtendida
    {
        public int IdPedido { get; set; }
        public int IdMesa { get; set; }
        public int IdEmpleado { get; set; }
        public string OrigenMesa { get; set; }
        public System.DateTime FechaHora { get; set; }
        public int TotalMesas { get; set; }

        public string NumeroMesa { get; set; }
        public string NombreEmpleado { get; set; }
    }
}