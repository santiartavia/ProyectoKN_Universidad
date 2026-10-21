using System;
using System.Collections;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using AccesoADatos;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using RestauranteVistas.Controllers;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Metadata.Edm;

class ColibriColumn
{
    public string Tabla { get; set; }
    public string Columna { get; set; }
    public string Tipo { get; set; }
}

class ColibriReadSession : HttpSessionStateBase
{
    public readonly Dictionary<string, object> Values = new Dictionary<string, object>();
    public override object this[string key] { get { return Values.ContainsKey(key) ? Values[key] : null; } set { Values[key] = value; } }
}
class ColibriReadContext : HttpContextBase
{
    public readonly ColibriReadSession State = new ColibriReadSession();
    public override HttpSessionStateBase Session { get { return State; } }
}

// Solo lectura: comprueba los mapeos EF contra la base existente sin SaveChanges.
class CheckColibri
{
    static int Main(string[] args)
    {
        int failures = 0, passed = 0;
        using (var db = new ColibriDbContext())
        {
            var database = db.Database.SqlQuery<string>("SELECT DB_NAME()").Single();
            if (database != "COLIBRI" && !(args.Contains("--azure-validacion") && database.StartsWith("Colibri_AzureValidacion_"))) throw new Exception("Se esperaba COLIBRI o una base aislada de validación Azure.");
            if (args.Contains("--azure-validacion"))
            {
                var demoUsers = db.Usuarios.Where(u => u.NombreUsuario.StartsWith("demo.")).ToList();
                if (demoUsers.Count != 4 || demoUsers.Any(u => !LogicaDeNegocios.Helpers.PasswordHelper.VerifyPassword("ColibriDemo2026!", u.PasswordHash)))
                    throw new Exception("Las credenciales demo no coinciden con PasswordHelper.");
                Console.WriteLine("OK hashes de las cuatro cuentas demo");
            }
            // Compara tipos incluso en tablas vacías: leer cero filas no detecta un INT mapeado como decimal.
            var columns = db.Database.SqlQuery<ColibriColumn>("SELECT TABLE_NAME Tabla, COLUMN_NAME Columna, DATA_TYPE Tipo FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'dbo'").ToList();
            var workspace = ((IObjectContextAdapter)db).ObjectContext.MetadataWorkspace;
            foreach (var table in workspace.GetItems<EntityContainer>(DataSpace.SSpace).SelectMany(c => c.BaseEntitySets).OfType<EntitySet>())
            {
                string name = table.Table ?? table.Name;
                foreach (var column in table.ElementType.Properties)
                {
                    var actual = columns.FirstOrDefault(c => string.Equals(c.Tabla, name, StringComparison.OrdinalIgnoreCase) && string.Equals(c.Columna, column.Name, StringComparison.OrdinalIgnoreCase));
                    if (actual != null && ClrType(actual.Tipo) != ClrType(column.TypeUsage.EdmType.Name))
                    {
                        Console.WriteLine("ERROR tipo " + name + "." + column.Name + ": SQL=" + actual.Tipo + ", modelo=" + column.TypeUsage.EdmType.Name);
                        failures++;
                    }
                }
            }
            foreach (var property in typeof(ColibriDbContext).GetProperties().Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)))
            {
                if (!db.PermiteSubcuentasVenta && (property.Name == "SubcuentasVenta" || property.Name == "SubcuentaDetallesVenta")) continue;
                if (!db.PermiteEnviosPdv && property.Name == "EnviosPdv") continue;
                try
                {
                    var query = (IQueryable)property.GetValue(db, null);
                    var take = Expression.Call(typeof(Queryable), "Take", new[] { query.ElementType }, query.Expression, Expression.Constant(1));
                    foreach (var item in query.Provider.CreateQuery(take)) { }
                    Console.WriteLine("OK " + property.Name); passed++;
                }
                catch (Exception ex) { Console.WriteLine("ERROR " + property.Name + ": " + ex.GetBaseException().Message); failures++; }
            }
            Console.WriteLine(database + ": " + passed + " mapeos correctos, " + failures + " errores. Solo lectura.");
            var user = db.Usuarios.First(u => u.Estado && u.Rol.NombreRol == "Administrador");
            var context = new ColibriReadContext();
            context.State["UsuarioId"] = user.IdUsuario;
            context.State["UsuarioNombre"] = user.NombreUsuario;
            context.State["RolNombre"] = "Administrador";
            var employee = db.Empleados.FirstOrDefault(e => e.IdUsuario == user.IdUsuario);
            if (employee != null) context.State["EmpleadoId"] = employee.IdEmpleado;
            foreach (var type in new[] { typeof(HomeController), typeof(MesasController), typeof(PedidosController), typeof(CocinaController), typeof(CajeroController), typeof(PuntoVentaController), typeof(InventarioController) })
            {
                using (var controller = (Controller)Activator.CreateInstance(type))
                {
                    controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
                    controller.TempData = new TempDataDictionary();
                    try
                    {
                        var action = type.GetMethod("Index");
                        action.Invoke(controller, action.GetParameters().Select(p => p.HasDefaultValue ? p.DefaultValue : null).ToArray());
                        Console.WriteLine("OK datos de " + type.Name);
                    }
                    catch (Exception ex) { Console.WriteLine("ERROR " + type.Name + ": " + ex.GetBaseException().Message); failures++; }
                }
            }
            if (args.Contains("--pedido"))
            {
                try { ProbarPedido(db, context); Console.WriteLine("OK crear, leer, editar y cancelar pedido; transacción revertida"); }
                catch (Exception ex) { Console.WriteLine("ERROR pedido: " + ex); failures++; }
            }
        }
        return failures == 0 ? 0 : 1;
    }

    static void ProbarPedido(ColibriDbContext original, ColibriReadContext context)
    {
        int before = original.Pedidos.Count();
        var stocks = original.Insumos.AsNoTracking().ToDictionary(i => i.IdInsumo, i => i.StockActual);
        using (var controller = new PedidosController())
        {
            controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
            controller.TempData = new TempDataDictionary();
            var db = (ColibriDbContext)typeof(OperacionController).GetField("db", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(controller);
            using (var transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.Serializable))
            {
                try
                {
                    var mesa = db.Mesas.First(m => m.Estado && m.EstadoMesa == "disponible" && !db.Pedidos.Any(p => p.IdMesa == m.IdMesa && p.Estado && p.EstadoPedido != "cancelado" && p.EstadoPedido != "finalizado"));
                    var empleado = db.Empleados.First(e => e.Estado && e.Usuario.Estado && e.Usuario.Rol.NombreRol == "Mesero");
                    var producto = db.Productos.First(p => p.Estado && p.Disponible);
                    string label = "Validación transaccional " + Guid.NewGuid().ToString("N");
                    controller.CrearOrden(mesa.IdMesa, empleado.IdEmpleado, 1, producto.IdProducto, 1, "mesa", label);
                    if (controller.TempData.ContainsKey("Error")) throw new Exception(Convert.ToString(controller.TempData["Error"]));
                    var pedido = db.Pedidos.AsNoTracking().Single(p => p.Observaciones == label);
                    var detalle = db.DetallePedidos.AsNoTracking().Single(d => d.IdPedido == pedido.IdPedido);
                    if (detalle.Cantidad != 1) throw new Exception("Cantidad guardada incorrecta");
                    controller.Index(); // Materializa también el detalle recién insertado.
                    controller.EditarProducto(detalle.IdDetalle, 2, "Edición de prueba revertida");
                    if (controller.TempData.ContainsKey("Error")) throw new Exception(Convert.ToString(controller.TempData["Error"]));
                    if (db.DetallePedidos.AsNoTracking().Single(d => d.IdDetalle == detalle.IdDetalle).Cantidad != 2) throw new Exception("No editó la cantidad");
                    controller.CancelarPedido(pedido.IdPedido, "Prueba con rollback");
                    if (controller.TempData.ContainsKey("Error")) throw new Exception(Convert.ToString(controller.TempData["Error"]));
                    if (db.Pedidos.AsNoTracking().Single(p => p.IdPedido == pedido.IdPedido).EstadoPedido != "cancelado") throw new Exception("No canceló el pedido");
                    foreach (var insumo in db.Insumos.AsNoTracking())
                        if (stocks[insumo.IdInsumo] != insumo.StockActual) throw new Exception("El inventario no se restauró al cancelar");
                }
                finally { transaction.Rollback(); }
            }
        }
        if (original.Pedidos.Count() != before) throw new Exception("La prueba dejó un pedido persistido");
        foreach (var insumo in original.Insumos.AsNoTracking())
            if (stocks[insumo.IdInsumo] != insumo.StockActual) throw new Exception("La prueba modificó el inventario");
    }

    static string ClrType(string type)
    {
        int suffix = type.IndexOf('(');
        if (suffix >= 0) type = type.Substring(0, suffix);
        switch (type.ToLowerInvariant())
        {
            case "nvarchar": case "varchar": case "nchar": case "char": case "text": case "ntext": return "string";
            case "date": case "datetime": case "datetime2": case "smalldatetime": return "DateTime";
            case "decimal": case "numeric": case "money": case "smallmoney": return "decimal";
            default: return type.ToLowerInvariant();
        }
    }
}
