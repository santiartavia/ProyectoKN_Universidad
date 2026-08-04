using Abstracciones.Models;
using System;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class OperacionPdvViewModel
    {
        public int VentasActivas { get; set; }
        public int VentasCobradas { get; set; }
        public decimal TotalCobrado { get; set; }
        public decimal TotalPendiente { get; set; }
        public decimal TiempoPromedioMinutos { get; set; }
        public string FiltroEstado { get; set; }
        public int? FiltroIdMesa { get; set; }
        public List<Mesa> Mesas { get; set; }

        public List<VentaOperacion> Ventas { get; set; }
        public List<ProductoOperacion> ProductosMasVendidos { get; set; }

        public class VentaOperacion
        {
            public int IdVenta { get; set; }
            public string Estado { get; set; }
            public decimal Total { get; set; }
            public DateTime FechaHora { get; set; }
            public bool Demorada { get; set; }
            public bool MuyEditada { get; set; }
            public int Ediciones { get; set; }
            public int? IdPedido { get; set; }
            public string Mesa { get; set; }
        }

        public class ProductoOperacion
        {
            public string Nombre { get; set; }
            public int Cantidad { get; set; }
        }
    }
}