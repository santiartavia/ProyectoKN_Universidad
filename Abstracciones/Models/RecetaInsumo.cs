namespace Abstracciones.Models
{
    public class RecetaInsumo
    {
        public int IdReceta { get; set; }
        public int IdInsumo { get; set; }
        public decimal CantidadUsar { get; set; }
        public Receta Receta { get; set; }
        public Insumo Insumo { get; set; }
    }
}
