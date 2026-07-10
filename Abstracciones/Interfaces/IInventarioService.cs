using Abstracciones.Models;
using System;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface IInventarioService
    {
        Insumo RegistrarInsumo(string nombreInsumo, int idCategoria, string unidadMedida, decimal stockMinimo, int idUsuario, int? idProveedor = null);
        List<Insumo> ListarInsumos(string busqueda = null, int? idCategoria = null, bool soloActivos = true);
        Insumo ObtenerInsumo(int idInsumo);
        Insumo ActualizarInsumo(int idInsumo, string nombreInsumo, int idCategoria, string unidadMedida, decimal stockMinimo, int idUsuario);
        void DesactivarInsumo(int idInsumo, int idUsuario);
        void ReactivarInsumo(int idInsumo, int idUsuario);
        void AjustarStock(int idInsumo, decimal cantidad, string tipoAjuste, string motivo, int idUsuario);
        List<CategoriaInsumo> ListarCategorias();
        List<Proveedor> ListarProveedores();
        List<BitacoraInventario> ConsultarBitacora(int? idUsuario = null, string accion = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        void ExportarBitacoraExcel(int? idUsuario, string accion, DateTime? fechaInicio, DateTime? fechaFin);
        List<string> ObtenerAccionesBitacora();
        List<Insumo> ListarInsumosBajoStock();
    }
}
