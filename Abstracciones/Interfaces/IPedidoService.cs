using Abstracciones.Models;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface IPedidoService
    {
        Pedido CrearPedido(int idMesa, int idEmpleado, byte comensales,
                           int idProducto, decimal cantidad, string tipoServicio,
                           string observaciones, int idUsuario);
        void AgregarProducto(int idPedido, int idProducto, decimal cantidad,
                             string observacionesItem, int idUsuario);
        void ModificarProducto(int idDetalle, decimal cantidad,
                               string observacionesItem, int idUsuario);
        void EliminarProducto(int idDetalle, int idUsuario);
        void CancelarPedido(int idPedido, string motivo, int idUsuario);
        List<BitacoraPedido> ConsultarBitacora(int? idPedido = null, string accion = null);
    }
}
