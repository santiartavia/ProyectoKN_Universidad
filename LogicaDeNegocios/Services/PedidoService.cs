using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class PedidoService : IPedidoService
    {
        private readonly IFechasLN _fechas;

        public PedidoService(IFechasLN fechas)
        {
            _fechas = fechas;
        }

        public Pedido CrearPedido(int idMesa, int idEmpleado, byte comensales,
                                  int idProducto, decimal cantidad, string tipoServicio,
                                  string observaciones, int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                string servicio = NormalizarTipoServicio(tipoServicio);

                var producto = ctx.Productos
                    .FirstOrDefault(p => p.IdProducto == idProducto && p.Estado && p.Disponible);
                if (producto == null)
                    throw new ArgumentException("El producto no existe o no esta disponible.");

                var pedido = new Pedido
                {
                    IdMesa = idMesa,
                    IdEmpleado = idEmpleado,
                    TipoServicio = servicio,
                    CantidadComensales = comensales,
                    EstadoPedido = "abierto",
                    FechaHora = _fechas.ObtenerFechaActual(),
                    Observaciones = observaciones,
                    Estado = true
                };

                ctx.Pedidos.Add(pedido);
                ctx.SaveChanges();

                ctx.DetallePedidos.Add(new DetallePedido
                {
                    IdPedido = pedido.IdPedido,
                    IdProducto = idProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.PrecioVenta,
                    ObservacionesItem = observaciones,
                    EstadoItem = "pendiente",
                    Estado = true
                });

                if (idUsuario > 0)
                {
                    ctx.BitacoraPedidos.Add(new BitacoraPedido
                    {
                        IdUsuario = idUsuario,
                        IdPedido = pedido.IdPedido,
                        Accion = "CREACION",
                        EstadoNuevo = "abierto",
                        Detalle = string.Format("Pedido creado: Mesa {0}, {1}x {2}",
                            idMesa, cantidad, producto.NombreProducto),
                        FechaHora = _fechas.ObtenerFechaActual()
                    });
                }

                ctx.SaveChanges();
                return pedido;
            }
        }

        public void AgregarProducto(int idPedido, int idProducto, decimal cantidad,
                                    string observacionesItem, int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                ValidarPedidoEditable(ctx, idPedido);

                var producto = ctx.Productos
                    .FirstOrDefault(p => p.IdProducto == idProducto && p.Estado && p.Disponible);
                if (producto == null)
                    throw new ArgumentException("El producto no existe o no esta disponible.");

                ctx.DetallePedidos.Add(new DetallePedido
                {
                    IdPedido = idPedido,
                    IdProducto = idProducto,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.PrecioVenta,
                    ObservacionesItem = observacionesItem,
                    EstadoItem = "pendiente",
                    Estado = true
                });

                if (idUsuario > 0)
                {
                    ctx.BitacoraPedidos.Add(new BitacoraPedido
                    {
                        IdUsuario = idUsuario,
                        IdPedido = idPedido,
                        Accion = "MODIFICACION",
                        Detalle = string.Format("Producto agregado: {0}x {1}",
                            cantidad, producto.NombreProducto),
                        FechaHora = _fechas.ObtenerFechaActual()
                    });
                }

                ctx.SaveChanges();
            }
        }

        public void ModificarProducto(int idDetalle, decimal cantidad,
                                      string observacionesItem, int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                var detalle = ctx.DetallePedidos.FirstOrDefault(d => d.IdDetalle == idDetalle && d.Estado);
                if (detalle == null)
                    throw new KeyNotFoundException("El detalle del pedido no existe.");

                ValidarPedidoEditable(ctx, detalle.IdPedido);

                detalle.Cantidad = cantidad;
                detalle.ObservacionesItem = observacionesItem;

                if (idUsuario > 0)
                {
                    ctx.BitacoraPedidos.Add(new BitacoraPedido
                    {
                        IdUsuario = idUsuario,
                        IdPedido = detalle.IdPedido,
                        Accion = "MODIFICACION",
                        Detalle = string.Format("Producto modificado: ID detalle {0}, cantidad {1}",
                            idDetalle, cantidad),
                        FechaHora = _fechas.ObtenerFechaActual()
                    });
                }

                ctx.SaveChanges();
            }
        }

        public void EliminarProducto(int idDetalle, int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                var detalle = ctx.DetallePedidos.FirstOrDefault(d => d.IdDetalle == idDetalle && d.Estado);
                if (detalle == null)
                    throw new KeyNotFoundException("El detalle del pedido no existe.");

                int idPedido = detalle.IdPedido;
                ValidarPedidoEditable(ctx, idPedido);

                detalle.Estado = false;

                if (idUsuario > 0)
                {
                    ctx.BitacoraPedidos.Add(new BitacoraPedido
                    {
                        IdUsuario = idUsuario,
                        IdPedido = idPedido,
                        Accion = "MODIFICACION",
                        Detalle = string.Format("Producto eliminado: ID detalle {0}", idDetalle),
                        FechaHora = _fechas.ObtenerFechaActual()
                    });
                }

                ctx.SaveChanges();

                bool hayProductosActivos = ctx.DetallePedidos
                    .Any(d => d.IdPedido == idPedido && d.Estado);
                if (!hayProductosActivos)
                {
                    CancelarPedidoInterno(ctx, idPedido,
                        "Cancelacion automatica: no quedan productos.", idUsuario);
                }
            }
        }

        public void CancelarPedido(int idPedido, string motivo, int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                CancelarPedidoInterno(ctx, idPedido, motivo, idUsuario);
            }
        }

        public List<BitacoraPedido> ConsultarBitacora(int? idPedido = null, string accion = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.BitacoraPedidos.AsQueryable();

                if (idPedido.HasValue)
                    query = query.Where(b => b.IdPedido == idPedido.Value);
                if (!string.IsNullOrWhiteSpace(accion))
                    query = query.Where(b => b.Accion == accion);

                return query.OrderByDescending(b => b.FechaHora).ToList();
            }
        }

        private void CancelarPedidoInterno(ColibriDbContext ctx, int idPedido,
                                           string motivo, int idUsuario)
        {
            var pedido = ctx.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado);
            if (pedido == null)
                throw new KeyNotFoundException("El pedido no existe.");

            string estadoAnterior = pedido.EstadoPedido;
            pedido.EstadoPedido = "cancelado";
            pedido.Observaciones = (pedido.Observaciones ?? "") + " | Cancelacion: " + motivo;

            var detalles = ctx.DetallePedidos.Where(d => d.IdPedido == idPedido && d.Estado).ToList();
            foreach (var item in detalles)
            {
                item.EstadoItem = "cancelado";
                item.Estado = false;
            }

            if (pedido.IdMesa.HasValue)
            {
                var mesa = ctx.Mesas.Find(pedido.IdMesa.Value);
                if (mesa != null)
                    mesa.EstadoMesa = "disponible";
            }

            if (idUsuario > 0)
            {
                ctx.BitacoraPedidos.Add(new BitacoraPedido
                {
                    IdUsuario = idUsuario,
                    IdPedido = idPedido,
                    Accion = "CANCELACION",
                    EstadoAnterior = estadoAnterior,
                    EstadoNuevo = "cancelado",
                    Detalle = "Cancelacion: " + motivo,
                    FechaHora = _fechas.ObtenerFechaActual()
                });
            }

            ctx.SaveChanges();
        }

        private void ValidarPedidoEditable(ColibriDbContext ctx, int idPedido)
        {
            var pedido = ctx.Pedidos.FirstOrDefault(p => p.IdPedido == idPedido && p.Estado);
            if (pedido == null)
                throw new KeyNotFoundException("El pedido no existe.");
            if (pedido.EstadoPedido != "abierto")
                throw new InvalidOperationException(
                    "No se puede modificar un pedido que no esta en estado abierto.");
        }

        private string NormalizarTipoServicio(string tipo)
        {
            if (tipo == "Salon" || tipo == "Salón")
                return "mesa";
            if (tipo == "Para llevar")
                return "para_llevar";
            if (tipo == "Delivery")
                return "delivery";
            return tipo;
        }
    }
}
