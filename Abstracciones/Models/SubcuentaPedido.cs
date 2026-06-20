namespace Abstracciones.Models
{
    public class SubcuentaPedido
    {
        public int IdSubcuenta { get; set; }
        public int IdPedido { get; set; }
        public string NombreSubcuenta { get; set; }
        public bool Estado { get; set; }
    }
}