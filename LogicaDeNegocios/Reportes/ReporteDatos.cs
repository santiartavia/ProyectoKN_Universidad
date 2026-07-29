using System.Collections.Generic;

namespace LogicaDeNegocios.Reportes
{
    public class ReporteFiltros
    {
        public int? CategoriaProducto { get; set; }
        public int? CategoriaInsumo { get; set; }
        public string MetodoPago { get; set; }
        public int? IdMesero { get; set; }
        public int? TopN { get; set; }
        public bool StockBajo { get; set; }
        public string TerminoBusqueda { get; set; }
        public string CategoriaEgreso { get; set; }
    }

    public class ReporteResultado
    {
        public string Titulo { get; set; }
        public string TipoReporte { get; set; }
        public string[] Headers { get; set; }
        public List<string[]> Rows { get; set; }
        public string MensajeVacio { get; set; }

        public bool TieneDatos
        {
            get { return Rows != null && Rows.Count > 0; }
        }
    }
}
