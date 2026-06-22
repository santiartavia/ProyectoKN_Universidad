using AccesoADatos;
using RestauranteVistas.Filters;
using RestauranteVistas.Models.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace RestauranteVistas.Controllers
{
    [AutorizacionFilter(RolesPermitidos = new[] { "Administrador" })]
    public class BitacoraPedidosController : Controller
    {
        [HttpGet]
        public ActionResult Index(int? idPedido, string fechaInicio, string fechaFin, string accion, int? idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.BitacoraPedidos.AsQueryable();

                if (idPedido.HasValue)
                    query = query.Where(b => b.IdPedido == idPedido.Value);
                if (idUsuario.HasValue)
                    query = query.Where(b => b.IdUsuario == idUsuario.Value);

                if (!string.IsNullOrWhiteSpace(fechaInicio))
                {
                    var fi = DateTime.Parse(fechaInicio);
                    query = query.Where(b => b.FechaHora >= fi);
                }
                if (!string.IsNullOrWhiteSpace(fechaFin))
                {
                    var ff = DateTime.Parse(fechaFin).AddDays(1);
                    query = query.Where(b => b.FechaHora < ff);
                }
                if (!string.IsNullOrWhiteSpace(accion))
                    query = query.Where(b => b.Accion == accion);

                var registros = query.OrderByDescending(b => b.FechaHora).ToList();

                var acciones = ctx.BitacoraPedidos
                    .Select(b => b.Accion)
                    .Distinct()
                    .OrderBy(a => a)
                    .ToList();

                var idsUsuarios = registros.Select(r => r.IdUsuario).Distinct().ToList();
                var dicUsuarios = ctx.Usuarios
                    .Where(u => idsUsuarios.Contains(u.IdUsuario))
                    .ToDictionary(u => u.IdUsuario, u => u.NombreUsuario);

                var modelo = new BitacoraPedidosViewModel
                {
                    Registros = registros,
                    Usuarios = dicUsuarios,
                    FiltroIdPedido = idPedido,
                    FiltroFechaInicio = fechaInicio,
                    FiltroFechaFin = fechaFin,
                    FiltroAccion = accion,
                    FiltroIdUsuario = idUsuario,
                    AccionesDisponibles = acciones
                };

                return View(modelo);
            }
        }

        [HttpGet]
        public ActionResult ExportarExcel(int? idPedido, string fechaInicio, string fechaFin, string accion, int? idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.BitacoraPedidos.AsQueryable();

                if (idPedido.HasValue)
                    query = query.Where(b => b.IdPedido == idPedido.Value);
                if (idUsuario.HasValue)
                    query = query.Where(b => b.IdUsuario == idUsuario.Value);

                if (!string.IsNullOrWhiteSpace(fechaInicio))
                {
                    var fi = DateTime.Parse(fechaInicio);
                    query = query.Where(b => b.FechaHora >= fi);
                }
                if (!string.IsNullOrWhiteSpace(fechaFin))
                {
                    var ff = DateTime.Parse(fechaFin).AddDays(1);
                    query = query.Where(b => b.FechaHora < ff);
                }
                if (!string.IsNullOrWhiteSpace(accion))
                    query = query.Where(b => b.Accion == accion);

                var registros = query.OrderByDescending(b => b.FechaHora).ToList();

                var idsUsuarios = registros.Select(r => r.IdUsuario).Distinct().ToList();
                var dicUsuarios = ctx.Usuarios
                    .Where(u => idsUsuarios.Contains(u.IdUsuario))
                    .ToDictionary(u => u.IdUsuario, u => u.NombreUsuario);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("ID,Usuario,Pedido,Accion,Estado Anterior,Estado Nuevo,Detalle,Fecha/Hora");
                foreach (var r in registros)
                {
                    string nombreUsuario = dicUsuarios.ContainsKey(r.IdUsuario) ? dicUsuarios[r.IdUsuario] : "";
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7:yyyy-MM-dd HH:mm:ss}",
                        r.IdRegistro, EscapeCsv(nombreUsuario),
                        r.IdPedido, r.Accion,
                        r.EstadoAnterior, r.EstadoNuevo,
                        EscapeCsv(r.Detalle), r.FechaHora));
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                return File(bytes, "text/csv", "bitacora_pedidos.csv");
            }
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
