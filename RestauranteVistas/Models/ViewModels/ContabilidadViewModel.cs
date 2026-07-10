using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class ContabilidadViewModel
    {
        public List<Caja> Cajas { get; set; }
        public List<AperturaCaja> Aperturas { get; set; }
        public List<CierreCaja> Cierres { get; set; }
        public List<EgresoCaja> Egresos { get; set; }
        public List<Venta> Ventas { get; set; }
        public List<NotaCredito> NotasCredito { get; set; }
        public List<ReporteGenerado> Reportes { get; set; }
        public AperturaCaja AperturaActiva { get; set; }

        public int? IdCajero { get; set; }
        public string[] CategoriasGasto { get; set; }

        public string Mensaje { get; set; }
        public string Error { get; set; }
        public string Alerta { get; set; }
    }
}
