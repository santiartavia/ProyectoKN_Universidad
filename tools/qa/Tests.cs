using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using AccesoADatos;
using Abstracciones.Models;
using LogicaDeNegocios.Helpers;
using LogicaDeNegocios.Services;
using LogicaDeNegocios.General.Fechas;
using RestauranteVistas.Controllers;

class MemorySession : HttpSessionStateBase
{
    readonly Dictionary<string, object> values = new Dictionary<string, object>();
    public override object this[string key] { get { return values.ContainsKey(key) ? values[key] : null; } set { values[key] = value; } }
    public override void Clear() { values.Clear(); }
    public override void Remove(string key) { values.Remove(key); }
    public override void Abandon() { values.Clear(); }
}
class TestRequest : HttpRequestBase
{
    public string Method = "POST";
    public readonly NameValueCollection Fields = new NameValueCollection();
    public override NameValueCollection Form { get { return Fields; } }
    public override string HttpMethod { get { return Method; } }
    public override string UserHostAddress { get { return "127.0.0.1"; } }
    public override string UserAgent { get { return "ColibriQA"; } }
}
class TestContext : HttpContextBase
{
    public readonly MemorySession UserSession = new MemorySession();
    public readonly TestRequest TestRequest = new TestRequest();
    public override HttpSessionStateBase Session { get { return UserSession; } }
    public override HttpRequestBase Request { get { return TestRequest; } }
}
class AuthProbe : RestauranteVistas.Filters.AutorizacionFilter
{
    public bool Check(HttpContextBase context) { return base.AuthorizeCore(context); }
}
class Tests
{
    static int passed, failed;
    static void Assert(bool ok, string message) { if (!ok) throw new Exception(message); }
    static void Test(string name, Action action)
    {
        try { action(); passed++; Console.WriteLine("APROBADA " + name); }
        catch (Exception ex) { failed++; Console.WriteLine("FALLIDA " + name + ": " + ex); }
    }
    static void Attach(Controller controller, TestContext context)
    {
        controller.ControllerContext = new ControllerContext(context, new RouteData(), controller);
        controller.TempData = new TempDataDictionary();
    }
    static void Execute(OperacionController controller, TestContext context, string action, Func<ActionResult> invoke, string key = null)
    {
        Attach(controller, context);
        context.TestRequest.Fields["requestKey"] = key ?? Guid.NewGuid().ToString();
        var descriptor = new ReflectedActionDescriptor(controller.GetType().GetMethod(action), action, new ReflectedControllerDescriptor(controller.GetType()));
        var before = new ActionExecutingContext(controller.ControllerContext, descriptor, new Dictionary<string, object>());
        typeof(OperacionController).GetMethod("OnActionExecuting", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(controller, new object[] { before });
        if (before.Result != null) return;
        var after = new ActionExecutedContext(controller.ControllerContext, descriptor, false, null);
        try { after.Result = invoke(); } catch (Exception ex) { after.Exception = ex; }
        typeof(OperacionController).GetMethod("OnActionExecuted", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(controller, new object[] { after });
    }
    static TestContext Session(int id, string role)
    {
        var ctx = new TestContext();
        ctx.UserSession["UsuarioId"] = id; ctx.UserSession["RolNombre"] = role; ctx.UserSession["UsuarioNombre"] = role;
        return ctx;
    }
    static void Unit()
    {
        Test("Excepción GET no redirige a la misma página", () => {
            var context = new TestContext(); context.TestRequest.Method = "GET";
            using (var c = new PedidosController()) {
                Attach(c, context);
                var descriptor = new ReflectedActionDescriptor(c.GetType().GetMethod("Index"), "Index", new ReflectedControllerDescriptor(c.GetType()));
                var after = new ActionExecutedContext(c.ControllerContext, descriptor, false, null) { Exception = new InvalidOperationException("Fallo de lectura") };
                typeof(OperacionController).GetMethod("OnActionExecuted", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(c, new object[] { after });
                Assert(!after.ExceptionHandled && !(after.Result is RedirectToRouteResult), "Ocultó el fallo GET con una redirección circular");
            }
        });
        var hash = PasswordHelper.HashPassword("PruebaSegura123!");
        Test("Contraseña correcta", () => Assert(PasswordHelper.VerifyPassword("PruebaSegura123!", hash), "No acepta la correcta"));
        Test("Contraseña incorrecta", () => Assert(!PasswordHelper.VerifyPassword("OtraClave123!", hash), "Aceptó una incorrecta"));
        Test("Salt aleatorio", () => Assert(hash != PasswordHelper.HashPassword("PruebaSegura123!"), "Salt repetido"));
        Test("Hash vacío", () => Assert(!PasswordHelper.VerifyPassword("admin", ""), "Aceptó vacío"));
        Test("Hash nulo", () => Assert(!PasswordHelper.VerifyPassword("admin", null), "Aceptó nulo"));
        Test("Hash corrupto", () => Assert(!PasswordHelper.VerifyPassword("admin", "%%%"), "Aceptó corrupto"));
        Test("Base64 reversible rechazado", () => Assert(!PasswordHelper.VerifyPassword("admin", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("admin"))), "Aceptó codificación reversible"));
        Test("Hash truncado", () => Assert(!PasswordHelper.VerifyPassword("admin", Convert.ToBase64String(new byte[16])), "Aceptó truncado"));
        Test("Política de contraseña", () => { string msg; Assert(!PasswordHelper.CumplePoliticaSeguridad("abc", out msg) && PasswordHelper.CumplePoliticaSeguridad("PruebaSegura123!", out msg), "Política incorrecta"); });
        Test("Compatibilidad PBKDF2 anterior", () => {
            var salt = new byte[16]; var bytes = new byte[48];
            using (var kdf = new System.Security.Cryptography.Rfc2898DeriveBytes("Anterior123!", salt, 10000, System.Security.Cryptography.HashAlgorithmName.SHA256)) Array.Copy(kdf.GetBytes(32), 0, bytes, 16, 32);
            Assert(PasswordHelper.VerifyPassword("Anterior123!", Convert.ToBase64String(bytes)) && !PasswordHelper.VerifyPassword("Incorrecta", Convert.ToBase64String(bytes)), "Falló hash previo");
        });
        Test("Correo inválido", () => Assert(!PasswordHelper.EsFormatoCorreoValido("sin-arroba"), "Aceptó correo inválido"));
        Test("Ruta privada sin sesión", () => Assert(!new AuthProbe().Check(new TestContext()), "Acceso sin sesión"));
        Test("Decimal HTML con punto en cultura es-CR", () => {
            var fields = new NameValueCollection(); fields["cantidad"] = "0.25";
            var model = new ModelBindingContext { ModelName = "cantidad", ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, typeof(decimal)), ValueProvider = new NameValueCollectionValueProvider(fields, System.Globalization.CultureInfo.GetCultureInfo("es-CR")) };
            Assert((decimal)new RestauranteVistas.Models.DecimalModelBinder().BindModel(new ControllerContext(), model) == .25m, "Interpretó mal un decimal HTML");
        });
        Test("Credencial temporal aleatoria y con política", () => {
            string msg; var one = PasswordHelper.GenerateTemporaryPassword();
            Assert(one != PasswordHelper.GenerateTemporaryPassword() && PasswordHelper.CumplePoliticaSeguridad(one, out msg), "Credencial temporal insegura");
        });
    }
    static void Provision()
    {
        var connection = Environment.GetEnvironmentVariable("COLIBRI_CONNECTION_STRING");
        Assert(new System.Data.SqlClient.SqlConnectionStringBuilder(connection).InitialCatalog == "Colibri_QA_20261020", "Solo se permite la base aislada QA.");
        using (var db = new ColibriDbContext())
        {
            if (!db.Database.Exists()) db.Database.Create();
            else
            {
                Assert(db.Database.SqlQuery<int>("SELECT COUNT(*) FROM sys.tables WHERE is_ms_shipped = 0").Single() == 0,
                    "La base QA ya tiene tablas; no se reemplaza ni borra.");
                var schema = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)db).ObjectContext.CreateDatabaseScript();
                db.Database.ExecuteSqlCommand(schema);
            }
        }
        Console.WriteLine("Base aislada QA creada desde el modelo EF, sin modificar bases existentes.");
    }
    static void Integration()
    {
        using (var db = new ColibriDbContext())
            db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction,
                System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../BD/qa_integridad.sql")));
        int admin, waiter, cook, cashier, employeeWaiter, employeeCashier, mesa, producto, insumo;
        string suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        string username = "qa" + suffix;
        using (var db = new ColibriDbContext())
        {
            var roles = new[] { "Administrador", "Mesero", "Cocinero", "Cajero" }.Select(n => new Rol { NombreRol = n + suffix, Estado = true }).ToArray();
            // Reuse canonical roles, keeping independent users and data per execution.
            foreach (var role in roles) { role.NombreRol = role.NombreRol.Replace(suffix, ""); var existing = db.Roles.FirstOrDefault(r => r.NombreRol == role.NombreRol); if (existing != null) role.IdRol = existing.IdRol; else db.Roles.Add(role); }
            db.SaveChanges();
            var users = roles.Select((r, i) => new Usuario { IdRol = r.IdRol, NombreUsuario = username + i, Correo = username + i + "@qa.test", PasswordHash = PasswordHelper.HashPassword("PruebaSegura123!"), Estado = true, FechaCreacion = DateTime.Now, FechaPassword = DateTime.Now }).ToArray();
            db.Usuarios.AddRange(users); db.SaveChanges();
            admin = users[0].IdUsuario; waiter = users[1].IdUsuario; cook = users[2].IdUsuario; cashier = users[3].IdUsuario;
            var employees = users.Select((u, i) => new Empleado { IdUsuario = u.IdUsuario, Cedula = suffix + i, Nombre = "QA", Apellidos = "Pruebas", SalarioHora = 2500, Estado = true, FechaIngreso = DateTime.Today }).ToArray();
            db.Empleados.AddRange(employees); db.SaveChanges(); employeeWaiter = employees[1].IdEmpleado; employeeCashier = employees[3].IdEmpleado;
            var table = new Mesa { NumeroMesa = suffix, Capacidad = 4, Estado = true, EstadoMesa = "disponible" }; db.Mesas.Add(table);
            var category = new CategoriaProducto { NombreCategoria = "QA " + suffix, Estado = true }; db.CategoriasProducto.Add(category);
            var cat = new CategoriaInsumo { NombreCategoria = "QA " + suffix, Estado = true }; db.CategoriasInsumo.Add(cat); db.SaveChanges();
            var milk = new Insumo { NombreInsumo = "Leche " + suffix, IdCategoria = cat.IdCategoria, UnidadMedida = "L", StockActual = 10m, StockMinimo = 1m, Estado = true }; db.Insumos.Add(milk);
            var glass = new Producto { NombreProducto = "Vaso de Leche " + suffix, IdCategoriaProd = category.IdCategoriaProd, PrecioVenta = 1000, Disponible = true, Estado = true }; db.Productos.Add(glass); db.SaveChanges();
            var recipe = new Receta { IdProducto = glass.IdProducto, Estado = true }; db.Recetas.Add(recipe); db.SaveChanges();
            db.RecetaInsumos.Add(new RecetaInsumo { IdReceta = recipe.IdReceta, IdInsumo = milk.IdInsumo, CantidadUsar = 0.25m });
            var caja = new Caja { NombreCaja = "QA " + suffix, Estado = true, EstadoCaja = "abierta" }; db.Cajas.Add(caja); db.SaveChanges();
            db.AperturasCaja.Add(new AperturaCaja { IdCaja = caja.IdCaja, IdCajero = employeeCashier, MontoInicial = 10000m, FechaApertura = DateTime.Now, Estado = true }); db.SaveChanges();
            mesa = table.IdMesa; producto = glass.IdProducto; insumo = milk.IdInsumo;
        }
        Test("D-01 administrador creado: contraseña incorrecta rechazada", () => {
            var session = new TestContext(); using (var c = new LoginController()) { Attach(c, session); c.Entrar(username + "0", "CualquierOtraClave"); Assert(session.Session["UsuarioId"] == null, "Abrió sesión con contraseña incorrecta"); }
        });
        Test("D-01 administrador creado: contraseña correcta aceptada", () => {
            var session = new TestContext(); using (var c = new LoginController()) { Attach(c, session); c.Entrar(username + "0", "PruebaSegura123!"); Assert((int?)session.Session["UsuarioId"] == admin && session.Session["IdSesion"] != null, "No abrió sesión válida"); }
        });
        Test("GUS 006 no permite omitir cambio seguro por CrearPassword", () => {
            var session = Session(admin, "Administrador"); using (var c = new LoginController()) { Attach(c, session); var result = (RedirectToRouteResult)c.CrearPassword("OtraClaveSegura123!", "OtraClaveSegura123!"); Assert((string)result.RouteValues["action"] == "Index", "El endpoint aceptó cambiar sin flujo autorizado"); }
            using (var db = new ColibriDbContext()) Assert(PasswordHelper.VerifyPassword("PruebaSegura123!", db.Usuarios.Find(admin).PasswordHash), "Cambió la contraseña sin autorización");
        });
        Test("D-01 administrador registrado por el servicio real", () => {
            int role; using (var db = new ColibriDbContext()) role = db.Usuarios.Find(admin).IdRol;
            var created = new UsuarioService(new FechasLN()).CrearUsuario(username + "prod", username + "prod@qa.test", "PruebaSegura123!", role, admin);
            Assert(created.PasswordHash.StartsWith("pbkdf2-sha256$"), "No guardó un hash seguro");
            var bad = new TestContext(); using (var c = new LoginController()) { Attach(c, bad); c.Entrar(created.NombreUsuario, "CualquierOtraClave"); Assert(bad.Session["UsuarioId"] == null, "Aceptó contraseña incorrecta para administrador nuevo"); }
            var good = new TestContext(); using (var c = new LoginController()) { Attach(c, good); c.Entrar(created.NombreUsuario, "PruebaSegura123!"); Assert((int?)good.Session["UsuarioId"] == created.IdUsuario, "No acepta las credenciales recién registradas"); }
        });
        Test("C1 expiración de sesión validada en backend", () => {
            var context = Session(admin, "Administrador");
            using (var db = new ColibriDbContext()) { var old = new Sesion { IdUsuario = admin, EstadoSesion = "ACTIVA", FechaHoraInicio = new FechasLN().ObtenerFechaActual().AddHours(-1), FechaHoraUltimaAct = new FechasLN().ObtenerFechaActual().AddMinutes(-31) }; db.Sesiones.Add(old); db.SaveChanges(); context.Session["IdSesion"] = old.IdSesion; }
            Assert(!new AuthProbe().Check(context) && context.Session["UsuarioId"] == null, "No expiró la sesión inactiva");
        });
        Test("D-02 asignación persistente", () => {
            using (var c = new MesasController()) Execute(c, Session(admin, "Administrador"), "AsignarMesero", () => c.AsignarMesero(mesa, employeeWaiter));
            using (var db = new ColibriDbContext()) Assert(db.Mesas.Find(mesa).IdEmpleadoAsignado == employeeWaiter, "Asignación no persistida");
        });
        string key = Guid.NewGuid().ToString(); int pedido = 0;
        Test("D-03 pedido, detalle, mesa y receta atómicos", () => {
            using (var c = new PedidosController()) { Execute(c, Session(waiter, "Mesero"), "CrearOrden", () => c.CrearOrden(mesa, employeeWaiter, 2, producto, 2, "mesa", "QA"), key); Assert(!c.TempData.ContainsKey("Error"), Convert.ToString(c.TempData["Error"])); }
            using (var db = new ColibriDbContext()) { pedido = db.Pedidos.Single(p => p.IdMesa == mesa).IdPedido; Assert(db.DetallePedidos.Single(d => d.IdPedido == pedido).Cantidad == 2 && db.Insumos.Find(insumo).StockActual == 9.5m, "Detalle o rebajo incorrecto"); }
        });
        Test("D-03 reintento no duplica pedido ni rebajo", () => {
            using (var c = new PedidosController()) Execute(c, Session(waiter, "Mesero"), "CrearOrden", () => c.CrearOrden(mesa, employeeWaiter, 2, producto, 2, "mesa", "QA"), key);
            using (var db = new ColibriDbContext()) Assert(db.Pedidos.Count(p => p.IdMesa == mesa) == 1 && db.Insumos.Find(insumo).StockActual == 9.5m, "Duplicó una solicitud");
        });
        Test("D-08 falta de stock revierte detalle y solicitud", () => {
            using (var c = new PedidosController()) { Execute(c, Session(waiter, "Mesero"), "AgregarProducto", () => c.AgregarProducto(pedido, producto, 100, "QA")); Assert(c.TempData.ContainsKey("Error"), "No informó falta de stock"); }
            using (var db = new ColibriDbContext()) Assert(db.DetallePedidos.Count(d => d.IdPedido == pedido) == 1 && db.Insumos.Find(insumo).StockActual == 9.5m, "No revirtió operación");
        });
        Test("C6 preparación y listo", () => {
            using (var c = new CocinaController()) Execute(c, Session(cook, "Cocinero"), "IniciarPreparacion", () => c.IniciarPreparacion(pedido));
            using (var db = new ColibriDbContext()) Assert(db.Pedidos.Find(pedido).EstadoPedido == "en_proceso", "No inició preparación");
            using (var c = new CocinaController()) Execute(c, Session(cook, "Cocinero"), "MarcarPedidoListo", () => c.MarcarPedidoListo(pedido));
            using (var db = new ColibriDbContext()) Assert(db.Pedidos.Find(pedido).EstadoPedido == "listo", "No quedó listo");
        });
        Test("D-04 entrega persiste y audita", () => {
            using (var c = new MeseroController()) Execute(c, Session(waiter, "Mesero"), "EntregarPedido", () => c.EntregarPedido(pedido));
            using (var db = new ColibriDbContext()) Assert(db.Pedidos.Find(pedido).EstadoPedido == "entregado" && db.BitacoraPedidos.Any(b => b.IdPedido == pedido && b.Accion == "ENTREGA"), "Entrega o auditoría faltante");
        });
        Test("D-06 monto insuficiente no registra pago", () => {
            using (var c = new CajeroController()) Execute(c, Session(cashier, "Cajero"), "RegistrarPago", () => c.RegistrarPago(pedido, "efectivo", 100));
            using (var db = new ColibriDbContext()) Assert(!db.Ventas.Any(v => v.IdPedido == pedido), "Registró pago insuficiente");
        });
        Test("D-06 cobro y reintento sin duplicación", () => {
            for (int i = 0; i < 2; i++) using (var c = new CajeroController()) Execute(c, Session(cashier, "Cajero"), "RegistrarPago", () => c.RegistrarPago(pedido, "efectivo", 2500));
            using (var db = new ColibriDbContext()) { var v = db.Ventas.Single(x => x.IdPedido == pedido); Assert(v.TotalCobrado == 2000m && v.Vuelto == 500m && v.IdApertura.HasValue, "Cobro o caja incorrectos"); }
        });
        Test("Finalización libera mesa con pago completo", () => {
            using (var c = new CajeroController()) Execute(c, Session(cashier, "Cajero"), "FinalizarOrden", () => c.FinalizarOrden(pedido));
            using (var db = new ColibriDbContext()) Assert(db.Pedidos.Find(pedido).EstadoPedido == "finalizado" && db.Mesas.Find(mesa).EstadoMesa == "disponible", "No finalizó correctamente");
        });
        Test("D-09 consulta y filtro de bitácora", () => {
            using (var c = new BitacoraPedidosController()) { Attach(c, Session(admin, "Administrador")); var result = (ViewResult)c.Index(pedido, null, null, null, null); var model = (RestauranteVistas.Models.ViewModels.BitacoraPedidosViewModel)result.Model; Assert(model.Registros.Any() && model.Registros.All(b => b.IdPedido == pedido) && c.ViewBag.Error == null, "Consulta vacía o incorrecta"); }
        });
        Test("D-05 cobro conserva productos para reportería", () => {
            using (var db = new ColibriDbContext()) { var venta = db.Ventas.Single(v => v.IdPedido == pedido); Assert(db.DetallesVenta.Where(d => d.IdVenta == venta.IdVenta && d.Estado).Sum(d => d.SubtotalItem) == venta.TotalCobrado, "Faltan detalles de factura"); }
        });
        Test("D-07 división, pagos individuales y cierre", () => {
            using (var c = new PedidosController()) Execute(c, Session(waiter, "Mesero"), "CrearOrden", () => c.CrearOrden(mesa, employeeWaiter, 2, producto, 2, "mesa", "Subcuentas"));
            int order, detail;
            using (var db = new ColibriDbContext()) { order = db.Pedidos.Where(p => p.IdMesa == mesa).Max(p => p.IdPedido); detail = db.DetallePedidos.Single(d => d.IdPedido == order).IdDetalle; }
            using (var c = new CocinaController()) Execute(c, Session(cook, "Cocinero"), "IniciarPreparacion", () => c.IniciarPreparacion(order));
            using (var c = new CocinaController()) Execute(c, Session(cook, "Cocinero"), "MarcarPedidoListo", () => c.MarcarPedidoListo(order));
            using (var c = new MeseroController()) Execute(c, Session(waiter, "Mesero"), "EntregarPedido", () => c.EntregarPedido(order));
            using (var c = new CajeroController()) Execute(c, Session(cashier, "Cajero"), "CrearDivisionSubcuentas", () => c.CrearDivisionSubcuentas(order, 2));
            int[] sub;
            using (var db = new ColibriDbContext()) sub = db.SubcuentasPedido.Where(s => s.IdPedido == order).Select(s => s.IdSubcuenta).ToArray();
            Assert(sub.Length == 2, "No creó dos subcuentas");
            foreach (int id in sub) using (var c = new CajeroController()) Execute(c, Session(cashier, "Cajero"), "AsignarProductoASubcuentaPedido", () => c.AsignarProductoASubcuentaPedido(order, id, detail, 1));
            foreach (int id in sub) using (var c = new CajeroController()) { Execute(c, Session(cashier, "Cajero"), "CobrarSubcuentaPedido", () => c.CobrarSubcuentaPedido(order, id, "tarjeta", 1000)); Assert(!c.TempData.ContainsKey("Error"), Convert.ToString(c.TempData["Error"])); }
            using (var db = new ColibriDbContext()) Assert(db.Pedidos.Find(order).EstadoPedido == "finalizado" && db.Ventas.Where(v => v.IdPedido == order && v.EstadoVenta == "completada").Sum(v => v.TotalCobrado) == 2000m, "Suma o cierre incorrectos");
        });
        Test("C2 sesión revocada y rol comprobados en backend", () => {
            var session = new TestContext(); using (var c = new LoginController()) { Attach(c, session); c.Entrar(username + "1", "PruebaSegura123!"); }
            var probe = new AuthProbe { RolesPermitidos = new[] { "Administrador" } };
            Assert(!probe.Check(session), "Mesero pudo acceder a administración");
            using (var db = new ColibriDbContext()) { var s = db.Sesiones.Find((int)session.Session["IdSesion"]); s.EstadoSesion = "CERRADA"; db.SaveChanges(); }
            Assert(!new AuthProbe().Check(session) && session.Session["UsuarioId"] == null, "Sesión revocada sigue activa");
        });
        Test("C3 edición de empleado conserva historial", () => {
            using (var db = new ColibriDbContext()) { var emp = db.Empleados.Find(employeeWaiter); emp.Telefono = "8888-7777"; db.SaveChanges(); }
            var service = new EmpleadoService(new AuditoriaService(new FechasLN()), new FechasLN());
            Assert(service.ObtenerPorId(employeeWaiter).Telefono == "8888-7777", "No persistió edición");
        });
        Test("C4 turno y asistencia persistentes", () => {
            var fechas = new FechasLN(); var audit = new AuditoriaService(fechas);
            var service = new TurnoService(audit, fechas); var day = fechas.ObtenerFechaActual().Date.AddDays(1);
            var turno = service.Crear(employeeWaiter, day, TimeSpan.FromHours(8), TimeSpan.FromHours(16), "QA", admin);
            Assert(service.ObtenerPorEmpleado(employeeWaiter).Any(t => t.IdTurno == turno.IdTurno), "Turno no persistió");
            var attendance = new AsistenciaService(audit, fechas); var entrada = attendance.RegistrarEntrada(employeeWaiter);
            Assert(attendance.ObtenerPorId(entrada.IdAsistencia).IdEmpleado == employeeWaiter, "Asistencia no persistió");
        });
        Test("C10 alerta en stock igual al mínimo", () => {
            using (var db = new ColibriDbContext()) { var milk = db.Insumos.Find(insumo); milk.StockMinimo = milk.StockActual; db.SaveChanges(); }
            Assert(new InventarioService(new FechasLN()).ListarInsumosBajoStock().Any(i => i.IdInsumo == insumo), "No detecta igualdad con mínimo");
        });
        Test("C11 apertura real, egreso y cierre", () => {
            var service = new ContabilidadService(new FechasLN()); int box;
            using (var db = new ColibriDbContext()) { var caja = new Caja { NombreCaja = "QA cierre " + suffix, Estado = true, EstadoCaja = "cerrada" }; db.Cajas.Add(caja); db.SaveChanges(); box = caja.IdCaja; }
            // Use the administrator's employee to keep the operational cashier's box open for UI tests.
            int emp; using (var db = new ColibriDbContext()) emp = db.Empleados.Single(e => e.IdUsuario == admin).IdEmpleado;
            var apertura = service.AbrirCaja(box, emp, 5000m, "QA", admin);
            service.RegistrarEgreso(apertura.IdApertura, admin, "Otros", "QA", 1000m);
            var cierre = service.CerrarCaja(apertura.IdApertura, emp, 4000m, admin);
            using (var db = new ColibriDbContext()) Assert(db.CierresCaja.Any(c => c.IdCierre == cierre.IdCierre) && db.Cajas.Find(box).EstadoCaja == "cerrada", "No cerró caja");
        });
        Test("C12 reporte y exportación contienen la venta", () => {
            var report = new LogicaDeNegocios.Reportes.ReporteService().Generar("ventas", null, null, new LogicaDeNegocios.Reportes.ReporteFiltros());
            var csv = System.Text.Encoding.UTF8.GetString(LogicaDeNegocios.Reportes.ExportadorReportes.GenerarCsv(report));
            Assert(report.TieneDatos && csv.Contains(pedido.ToString()), "Reporte o archivo sin datos esperados");
        });
        Test("C13 nómina calcula con salario registrado", () => {
            var fechas = new FechasLN(); var now = fechas.ObtenerFechaActual();
            var service = new NominaMensualService(new AuditoriaService(fechas), fechas);
            var payroll = service.CalcularPreview(now.Month, now.Year).Single(n => n.IdEmpleado == employeeWaiter);
            Assert(payroll.ValorHora == 2500m && payroll.SalarioBruto == payroll.SalarioBase + payroll.MontoHorasExtra + payroll.MontoVacaciones, "Cálculo inconsistente");
        });
        Test("C14 bitácora inmutable en base de datos", () => {
            bool denied = false;
            try { using (var db = new ColibriDbContext()) db.Database.ExecuteSqlCommand("UPDATE dbo.Bitacora_Pedidos SET detalle = detalle WHERE id_pedido = @p0", pedido); }
            catch (System.Data.SqlClient.SqlException ex) { denied = ex.Number == 51010; }
            Assert(denied, "La base permite alterar la bitácora");
        });
        System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fixture.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new {
            admin = username + "0", mesero = username + "1", cocinero = username + "2", cajero = username + "3",
            password = "PruebaSegura123!", idMesa = mesa, idProducto = producto, idInsumo = insumo, idMesero = employeeWaiter,
            idCajero = employeeCashier, idPedido = pedido, suffix = suffix
        }));
    }
    static int Main(string[] args)
    {
        try { if (args.Contains("--provision")) { Provision(); return 0; } if (args.Contains("--integration")) Integration(); else Unit(); }
        catch (Exception ex) { failed++; Console.WriteLine("BLOQUEO O FALLO: " + ex); }
        Console.WriteLine("RESULTADO: " + passed + " aprobadas, " + failed + " fallidas.");
        return failed == 0 ? 0 : 1;
    }
}
