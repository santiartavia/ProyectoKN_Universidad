namespace Abstracciones.Models
{
    public class Insumo
    {
        public int IdInsumo { get; set; }
        public int IdCategoria { get; set; }
        public string NombreInsumo { get; set; }
        public string UnidadMedida { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal StockActual { get; set; }
        public decimal CostoUnitario { get; set; }
        public bool Estado { get; set; }
        public CategoriaInsumo Categoria { get; set; }
    }
}
