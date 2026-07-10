using System;

namespace Abstracciones.Models
{
    public class InsumoProveedor
    {
        public int IdInsumo { get; set; }
        public int IdProveedor { get; set; }
        public DateTime FechaAsoc { get; set; }
        public Insumo Insumo { get; set; }
        public Proveedor Proveedor { get; set; }
    }
}
