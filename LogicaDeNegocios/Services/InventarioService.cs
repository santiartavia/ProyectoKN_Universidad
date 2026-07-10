using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class InventarioService : IInventarioService
    {
        private readonly IFechasLN _fechas;

        public InventarioService(IFechasLN fechas)
        {
            _fechas = fechas;
        }

        public Insumo RegistrarInsumo(string nombreInsumo, int idCategoria, string unidadMedida, decimal stockMinimo, int idUsuario, int? idProveedor = null)
        {
            if (string.IsNullOrWhiteSpace(nombreInsumo))
                throw new ArgumentException("El nombre del insumo es obligatorio");
            if (idCategoria <= 0)
                throw new ArgumentException("La categoría es obligatoria");
if (string.IsNullOrWhiteSpace(unidadMedida) || !new[] { "Kg", "L", "Unidad", "ml", "oz", "g" }.Contains(unidadMedida))
    throw new ArgumentException("La unidad de medida debe ser Kg, L, Unidad, ml, oz o g");
            if (stockMinimo < 0)
                throw new ArgumentException("El stock mínimo no puede ser negativo");

            using (var ctx = new ColibriDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                if (ctx.Set<Insumo>().Any(i => i.NombreInsumo == nombreInsumo && i.Estado))
                    throw new InvalidOperationException("Ya existe un insumo con ese nombre");

                var insumo = new Insumo
                {
                    IdCategoria = idCategoria,
                    NombreInsumo = nombreInsumo,
                    UnidadMedida = unidadMedida,
                    StockMinimo = stockMinimo,
                    StockActual = 0,
                    CostoUnitario = 0,
                    Estado = true
                };
                ctx.Set<Insumo>().Add(insumo);
                ctx.SaveChanges();

                if (idProveedor.HasValue)
                {
                    var rel = new InsumoProveedor
                    {
                        IdInsumo = insumo.IdInsumo,
                        IdProveedor = idProveedor.Value,
                        FechaAsoc = _fechas.ObtenerFechaActual()
                    };
                    ctx.Set<InsumoProveedor>().Add(rel);
                    ctx.SaveChanges();
                }

                var bitacora = new BitacoraInventario
                {
                    IdUsuario = idUsuario,
                    Accion = "entrada",
                    ValorNuevo = $"Insumo creado: {nombreInsumo}",
                    Detalle = $"Registro de nuevo insumo {nombreInsumo} (Cat:{idCategoria}, UDM:{unidadMedida})",
                    FechaHora = _fechas.ObtenerFechaActual()
                };
                ctx.Set<BitacoraInventario>().Add(bitacora);
                ctx.SaveChanges();

                tx.Commit();
                return insumo;
            }
        }

        public List<Insumo> ListarInsumos(string busqueda = null, int? idCategoria = null, bool soloActivos = true)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.Set<Insumo>().Include(i => i.Categoria).AsQueryable();

                if (soloActivos)
                    query = query.Where(i => i.Estado);

                if (!string.IsNullOrWhiteSpace(busqueda))
                    query = query.Where(i => i.NombreInsumo.Contains(busqueda));

                if (idCategoria.HasValue)
                    query = query.Where(i => i.IdCategoria == idCategoria.Value);

                return query.OrderBy(i => i.NombreInsumo).ToList();
            }
        }

        public Insumo ObtenerInsumo(int idInsumo)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Set<Insumo>().Include(i => i.Categoria)
                    .FirstOrDefault(i => i.IdInsumo == idInsumo);
            }
        }

        public Insumo ActualizarInsumo(int idInsumo, string nombreInsumo, int idCategoria, string unidadMedida, decimal stockMinimo, int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                var insumo = ctx.Set<Insumo>().FirstOrDefault(i => i.IdInsumo == idInsumo);
                if (insumo == null)
                    throw new KeyNotFoundException("Insumo no encontrado");

                if (ctx.Set<Insumo>().Any(i => i.NombreInsumo == nombreInsumo && i.IdInsumo != idInsumo && i.Estado))
                    throw new InvalidOperationException("El nombre del insumo ya está en uso por otro registro");

                if (string.IsNullOrWhiteSpace(nombreInsumo))
                    throw new ArgumentException("El nombre del insumo no puede estar vacío");

                var valorAnterior = $"Nombre:{insumo.NombreInsumo}, Cat:{insumo.IdCategoria}, UDM:{insumo.UnidadMedida}, StkMin:{insumo.StockMinimo}";

                insumo.NombreInsumo = nombreInsumo;
                insumo.IdCategoria = idCategoria;
                insumo.StockMinimo = stockMinimo;
                ctx.SaveChanges();

                var bitacora = new BitacoraInventario
                {
                    IdUsuario = idUsuario,
                    Accion = "ajuste",
                    ValorAnterior = valorAnterior,
                    ValorNuevo = $"Nombre:{nombreInsumo}, Cat:{idCategoria}, UDM:{unidadMedida}, StkMin:{stockMinimo}",
                    Detalle = $"Modificación de insumo #{idInsumo} ({nombreInsumo})",
                    FechaHora = _fechas.ObtenerFechaActual()
                };
                ctx.Set<BitacoraInventario>().Add(bitacora);
                ctx.SaveChanges();

                return insumo;
            }
        }

        public void DesactivarInsumo(int idInsumo, int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                var insumo = ctx.Set<Insumo>().FirstOrDefault(i => i.IdInsumo == idInsumo);
                if (insumo == null)
                    throw new KeyNotFoundException("Insumo no encontrado");

                if (!insumo.Estado)
                    throw new InvalidOperationException("El insumo ya está inactivo");

                var productosEnRecetas = ctx.Set<RecetaInsumo>()
                    .Where(ri => ri.IdInsumo == idInsumo && ri.Receta.Estado)
                    .Include(ri => ri.Receta.Producto)
                    .Select(ri => ri.Receta.Producto.NombreProducto)
                    .Distinct()
                    .ToList();

                if (productosEnRecetas.Any())
                {
                    var productosStr = string.Join(", ", productosEnRecetas);
                    throw new InvalidOperationException(
                        $"No se puede desactivar el insumo porque está siendo utilizado en: {productosStr}.");
                }

                insumo.Estado = false;
                ctx.SaveChanges();

                var bitacora = new BitacoraInventario
                {
                    IdUsuario = idUsuario,
                    Accion = "baja",
                    ValorAnterior = "Estado: Activo",
                    ValorNuevo = "Estado: Inactivo",
                    Detalle = $"Desactivación de insumo #{idInsumo} ({insumo.NombreInsumo})",
                    FechaHora = _fechas.ObtenerFechaActual()
                };
                ctx.Set<BitacoraInventario>().Add(bitacora);
                ctx.SaveChanges();
            }
        }

        public void ReactivarInsumo(int idInsumo, int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                var insumo = ctx.Set<Insumo>().FirstOrDefault(i => i.IdInsumo == idInsumo);
                if (insumo == null)
                    throw new KeyNotFoundException("Insumo no encontrado");

                if (insumo.Estado)
                    throw new InvalidOperationException("El insumo ya está activo");

                insumo.Estado = true;
                ctx.SaveChanges();

                var bitacora = new BitacoraInventario
                {
                    IdUsuario = idUsuario,
                    Accion = "activacion",
                    ValorAnterior = "Estado: Inactivo",
                    ValorNuevo = "Estado: Activo",
                    Detalle = $"Reactivación de insumo #{idInsumo} ({insumo.NombreInsumo})",
                    FechaHora = _fechas.ObtenerFechaActual()
                };
                ctx.Set<BitacoraInventario>().Add(bitacora);
                ctx.SaveChanges();
            }
        }

        public void AjustarStock(int idInsumo, decimal cantidad, string tipoAjuste, string motivo, int idUsuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("Debe escribir un motivo para el ajuste");

            using (var ctx = new ColibriDbContext())
            {
                var insumo = ctx.Set<Insumo>().FirstOrDefault(i => i.IdInsumo == idInsumo);
                if (insumo == null)
                    throw new KeyNotFoundException("Insumo no encontrado");

                decimal valorAnterior = insumo.StockActual;
                string detalleBitacora;

                if (tipoAjuste == "entrada")
                {
                    insumo.StockActual += cantidad;
                    detalleBitacora = $"Entrada: +{cantidad} {insumo.UnidadMedida}. Motivo: {motivo}";
                }
                else if (tipoAjuste == "salida")
                {
                    if (cantidad > insumo.StockActual)
                        throw new InvalidOperationException($"El ajuste de salida ({cantidad}) supera el stock actual ({insumo.StockActual}). No se puede realizar la operación.");

                    insumo.StockActual -= cantidad;
                    detalleBitacora = $"Salida: -{cantidad} {insumo.UnidadMedida}. Motivo: {motivo}";

                    if (insumo.StockActual < insumo.StockMinimo)
                        detalleBitacora += " [ALERTA: Stock por debajo del mínimo]";
                }
                else
                {
                    throw new ArgumentException("El tipo de ajuste debe ser 'entrada' o 'salida'");
                }

                ctx.SaveChanges();

                var bitacora = new BitacoraInventario
                {
                    IdUsuario = idUsuario,
                    Accion = tipoAjuste,
                    ValorAnterior = valorAnterior.ToString("F2"),
                    ValorNuevo = insumo.StockActual.ToString("F2"),
                    Detalle = detalleBitacora,
                    FechaHora = _fechas.ObtenerFechaActual()
                };
                ctx.Set<BitacoraInventario>().Add(bitacora);
                ctx.SaveChanges();
            }
        }

        public List<CategoriaInsumo> ListarCategorias()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Set<CategoriaInsumo>().Where(c => c.Estado).OrderBy(c => c.NombreCategoria).ToList();
            }
        }

        public List<Proveedor> ListarProveedores()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Set<Proveedor>().Where(p => p.Estado).OrderBy(p => p.NombreEmpresa).ToList();
            }
        }

        public List<BitacoraInventario> ConsultarBitacora(int? idUsuario = null, string accion = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.Set<BitacoraInventario>().Include(b => b.Usuario).AsQueryable();

                if (idUsuario.HasValue)
                    query = query.Where(b => b.IdUsuario == idUsuario.Value);
                if (!string.IsNullOrWhiteSpace(accion))
                    query = query.Where(b => b.Accion == accion);
                if (fechaInicio.HasValue)
                    query = query.Where(b => b.FechaHora >= fechaInicio.Value);
                if (fechaFin.HasValue)
                    query = query.Where(b => b.FechaHora <= fechaFin.Value);

                return query.OrderByDescending(b => b.FechaHora).ToList();
            }
        }

        public void ExportarBitacoraExcel(int? idUsuario, string accion, DateTime? fechaInicio, DateTime? fechaFin)
        {
        }

        public List<string> ObtenerAccionesBitacora()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Set<BitacoraInventario>()
                    .Select(b => b.Accion)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList();
            }
        }

        public List<Insumo> ListarInsumosBajoStock()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Set<Insumo>().Include(i => i.Categoria)
                    .Where(i => i.Estado && i.StockActual < i.StockMinimo)
                    .OrderBy(i => i.NombreInsumo)
                    .ToList();
            }
        }
    }
}
