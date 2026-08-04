using Abstracciones.Models;
using System.Collections.Generic;

namespace RestauranteVistas.Models.ViewModels
{
    public class PuntoVentaViewModel
    {
        public bool TieneAperturaActiva { get; set; }
        public string NombreCajero { get; set; }
        public string Mensaje { get; set; }
        public string Error { get; set; }
        public string Alerta { get; set; }

        public List<Producto> Productos { get; set; }
        public List<VentaActiva> VentasActivas { get; set; }

        public class VentaActiva
        {
            public Venta Venta { get; set; }
            public List<DetalleVenta> Detalles { get; set; }
            public decimal Total { get; set; }
            public List<EnvioPdv> Envios { get; set; }
            public List<SubcuentaVenta> Subcuentas { get; set; }
            public List<SubcuentaVentaDetalle> SubcuentaItems { get; set; }
            public Dictionary<int, int> PendientePorDetalle { get; set; }
            public int Ediciones { get; set; }
            public bool MuyEditada { get; set; }
        }

        public class SubcuentaVentaDetalle
        {
            public int IdSubcuenta { get; set; }
            public string NombreSubcuenta { get; set; }
            public bool Pagada { get; set; }
            public int IdSubcuentaDetalle { get; set; }
            public int IdDetalleVenta { get; set; }
            public string NombreProducto { get; set; }
            public int Cantidad { get; set; }
            public decimal Subtotal { get; set; }
        }
    }
}