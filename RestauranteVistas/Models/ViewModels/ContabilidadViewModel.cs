using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class ContabilidadViewModel
    {
        public List<Caja> CajasCerradas { get; set; }
        public List<AperturaCaja> AperturasActivas { get; set; }
        public AperturaCaja AperturaActiva { get; set; }

        public List<Venta> Ventas { get; set; }
        public List<EgresoCaja> Egresos { get; set; }
        public List<NotaCredito> NotasCredito { get; set; }
        public List<NotaCredito> VentasCanceladas { get; set; }
        public decimal SaldoDisponible { get; set; }
        public decimal SaldoEsperado { get; set; }

        public List<Venta> VentasAnulables { get; set; }

        public List<CierreCaja> Cierres { get; set; }
        public List<ReporteGenerado> Reportes { get; set; }
        public List<CierrePeriodo> CierresPeriodo { get; set; }

        public int? IdCajero { get; set; }
        public string[] CategoriasGasto { get; set; }

        public List<BitacoraFinanciera> BitacoraFinanciera { get; set; }
        public List<string> AccionesBitacora { get; set; }
        public int? FiltroUsuario { get; set; }
        public string FiltroAccion { get; set; }
        public string FiltroFechaInicio { get; set; }
        public string FiltroFechaFin { get; set; }

        public string Mensaje { get; set; }
        public string Error { get; set; }
        public string Alerta { get; set; }
    }
}
