using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IFechasLN _fechas;

        public ProductoService(IFechasLN fechas)
        {
            _fechas = fechas;
        }

        public List<Producto> ListarProductos(string busqueda = null, int? idCategoria = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.Set<Producto>().AsQueryable();
                if (!string.IsNullOrWhiteSpace(busqueda))
                    query = query.Where(p => p.NombreProducto.Contains(busqueda));
                if (idCategoria.HasValue)
                    query = query.Where(p => p.IdCategoriaProd == idCategoria.Value);
                return query.OrderBy(p => p.NombreProducto).ToList();
            }
        }

        public Producto ObtenerProducto(int idProducto)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Set<Producto>().FirstOrDefault(p => p.IdProducto == idProducto);
            }
        }

        public Producto RegistrarProducto(string nombre, string descripcion, decimal precio, int idCategoria, bool disponible)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del producto es obligatorio");
            if (precio <= 0)
                throw new ArgumentException("El precio debe ser mayor a 0");
            if (idCategoria <= 0)
                throw new ArgumentException("La categoría es obligatoria");

            using (var ctx = new ColibriDbContext())
            {
                if (ctx.Set<Producto>().Any(p => p.NombreProducto == nombre && p.Estado))
                    throw new InvalidOperationException("Ya existe un producto con ese nombre");

                var producto = new Producto
                {
                    NombreProducto = nombre,
                    Descripcion = descripcion ?? "",
                    PrecioVenta = precio,
                    IdCategoriaProd = idCategoria,
                    Disponible = disponible,
                    Estado = true
                };
                ctx.Set<Producto>().Add(producto);
                ctx.SaveChanges();
                return producto;
            }
        }

        public Producto ActualizarProducto(int idProducto, string nombre, string descripcion, decimal precio, int idCategoria, bool disponible)
        {
            using (var ctx = new ColibriDbContext())
            {
                var producto = ctx.Set<Producto>().FirstOrDefault(p => p.IdProducto == idProducto);
                if (producto == null)
                    throw new KeyNotFoundException("Producto no encontrado");
                if (ctx.Set<Producto>().Any(p => p.NombreProducto == nombre && p.IdProducto != idProducto && p.Estado))
                    throw new InvalidOperationException("El nombre del producto ya está en uso");

                producto.NombreProducto = nombre;
                producto.Descripcion = descripcion ?? "";
                producto.PrecioVenta = precio;
                producto.IdCategoriaProd = idCategoria;
                producto.Disponible = disponible;
                ctx.SaveChanges();
                return producto;
            }
        }

        public void DesactivarProducto(int idProducto)
        {
            using (var ctx = new ColibriDbContext())
            {
                var producto = ctx.Set<Producto>().FirstOrDefault(p => p.IdProducto == idProducto);
                if (producto == null)
                    throw new KeyNotFoundException("Producto no encontrado");
                if (!producto.Estado)
                    throw new InvalidOperationException("El producto ya está inactivo");

                producto.Estado = false;

                var receta = ctx.Set<Receta>().FirstOrDefault(r => r.IdProducto == idProducto);
                if (receta != null && receta.Estado)
                    receta.Estado = false;

                ctx.SaveChanges();
            }
        }

        public void ReactivarProducto(int idProducto)
        {
            using (var ctx = new ColibriDbContext())
            {
                var producto = ctx.Set<Producto>().FirstOrDefault(p => p.IdProducto == idProducto);
                if (producto == null)
                    throw new KeyNotFoundException("Producto no encontrado");
                if (producto.Estado)
                    throw new InvalidOperationException("El producto ya está activo");

                producto.Estado = true;

                var receta = ctx.Set<Receta>().FirstOrDefault(r => r.IdProducto == idProducto);
                if (receta != null && !receta.Estado)
                    receta.Estado = true;

                ctx.SaveChanges();
            }
        }

        public List<CategoriaProducto> ListarCategoriasProducto()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Set<CategoriaProducto>().Where(c => c.Estado).OrderBy(c => c.NombreCategoria).ToList();
            }
        }

        public Receta ObtenerRecetaPorProducto(int idProducto)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Set<Receta>().Include(r => r.Producto).FirstOrDefault(r => r.IdProducto == idProducto);
            }
        }

        public List<RecetaInsumo> ObtenerRecetaInsumos(int idReceta)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Set<RecetaInsumo>()
                    .Where(ri => ri.IdReceta == idReceta)
                    .Include(ri => ri.Insumo)
                    .Include(ri => ri.Receta)
                    .ToList();
            }
        }

        public Receta CrearRecetaCompleta(int idProducto, int[] idInsumos, decimal[] cantidades)
        {
            if (idInsumos == null || idInsumos.Length == 0)
                throw new ArgumentException("Debe agregar al menos un insumo a la receta");
            if (cantidades == null || cantidades.Length != idInsumos.Length)
                throw new ArgumentException("Las cantidades no coinciden con los insumos seleccionados");

            using (var ctx = new ColibriDbContext())
            {
                if (ctx.Set<Receta>().Any(r => r.IdProducto == idProducto))
                    throw new InvalidOperationException("El producto ya tiene una receta registrada");

                var producto = ctx.Set<Producto>().FirstOrDefault(p => p.IdProducto == idProducto);
                if (producto == null)
                    throw new KeyNotFoundException("Producto no encontrado");

                using (var tx = ctx.Database.BeginTransaction())
                {
                    var receta = new Receta
                    {
                        IdProducto = idProducto,
                        Estado = true
                    };
                    ctx.Set<Receta>().Add(receta);
                    ctx.SaveChanges();

                    for (int i = 0; i < idInsumos.Length; i++)
                    {
                        var idInsumo = idInsumos[i];
                        var cantidad = cantidades[i];

                        if (cantidad <= 0)
                            throw new ArgumentException($"La cantidad del insumo #{i + 1} debe ser mayor a 0");

                        var insumo = ctx.Set<Insumo>().FirstOrDefault(inv => inv.IdInsumo == idInsumo);
                        if (insumo == null || !insumo.Estado)
                            throw new ArgumentException($"El insumo con ID {idInsumo} no existe o está inactivo");

                        if (ctx.Set<RecetaInsumo>().Any(ri => ri.IdReceta == receta.IdReceta && ri.IdInsumo == idInsumo))
                            throw new InvalidOperationException($"El insumo ya está registrado en esta receta");

                        var detalle = new RecetaInsumo
                        {
                            IdReceta = receta.IdReceta,
                            IdInsumo = idInsumo,
                            CantidadUsar = cantidad
                        };
                        ctx.Set<RecetaInsumo>().Add(detalle);
                    }
                    ctx.SaveChanges();
                    tx.Commit();
                    return receta;
                }
            }
        }

        public void AgregarInsumoReceta(int idReceta, int idInsumo, decimal cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a 0");

            using (var ctx = new ColibriDbContext())
            {
                var receta = ctx.Set<Receta>().FirstOrDefault(r => r.IdReceta == idReceta);
                if (receta == null)
                    throw new KeyNotFoundException("Receta no encontrada");

                if (ctx.Set<RecetaInsumo>().Any(ri => ri.IdReceta == idReceta && ri.IdInsumo == idInsumo))
                    throw new InvalidOperationException("Este insumo ya está registrado en la receta");

                var insumo = ctx.Set<Insumo>().FirstOrDefault(inv => inv.IdInsumo == idInsumo);
                if (insumo == null || !insumo.Estado)
                    throw new ArgumentException("El insumo no existe o está inactivo");

                var detalle = new RecetaInsumo
                {
                    IdReceta = idReceta,
                    IdInsumo = idInsumo,
                    CantidadUsar = cantidad
                };
                ctx.Set<RecetaInsumo>().Add(detalle);
                ctx.SaveChanges();
            }
        }

        public void EliminarInsumoReceta(int idReceta, int idInsumo)
        {
            using (var ctx = new ColibriDbContext())
            {
                var detalle = ctx.Set<RecetaInsumo>().FirstOrDefault(ri => ri.IdReceta == idReceta && ri.IdInsumo == idInsumo);
                if (detalle == null)
                    throw new KeyNotFoundException("El insumo no está en esta receta");

                ctx.Set<RecetaInsumo>().Remove(detalle);
                ctx.SaveChanges();
            }
        }

        public void EliminarRecetaCompleta(int idReceta)
        {
            using (var ctx = new ColibriDbContext())
            {
                var receta = ctx.Set<Receta>().Include(r => r.Producto).FirstOrDefault(r => r.IdReceta == idReceta);
                if (receta == null)
                    throw new KeyNotFoundException("Receta no encontrada");

                using (var tx = ctx.Database.BeginTransaction())
                {
                    var detalles = ctx.Set<RecetaInsumo>().Where(ri => ri.IdReceta == idReceta).ToList();
                    foreach (var d in detalles)
                        ctx.Set<RecetaInsumo>().Remove(d);

                    ctx.Set<Receta>().Remove(receta);

                    if (receta.Producto != null && !receta.Producto.Estado)
                    {
                        receta.Producto.Estado = true;
                    }

                    ctx.SaveChanges();
                    tx.Commit();
                }
            }
        }

        public void AlternarEstadoReceta(int idReceta)
        {
            using (var ctx = new ColibriDbContext())
            {
                var receta = ctx.Set<Receta>().Include(r => r.Producto).FirstOrDefault(r => r.IdReceta == idReceta);
                if (receta == null)
                    throw new KeyNotFoundException("Receta no encontrada");

                receta.Estado = !receta.Estado;

                if (receta.Producto != null)
                {
                    receta.Producto.Estado = receta.Estado;
                }

                ctx.SaveChanges();
            }
        }
    }
}
