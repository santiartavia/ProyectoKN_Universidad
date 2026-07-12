using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using LogicaDeNegocios.General.Fechas;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class ContabilidadService : IContabilidadService
    {
        private readonly IFechasLN _fechas;

        public ContabilidadService(IFechasLN fechas)
        {
            _fechas = fechas;
        }

        public AperturaCaja AbrirCaja(int idCaja, int idCajero, decimal montoInicial, string observaciones, int idUsuarioAdmin, string ip = null, string dispositivo = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var caja = ctx.Cajas.FirstOrDefault(c => c.IdCaja == idCaja && c.Estado);
                if (caja == null)
                    throw new KeyNotFoundException("La caja especificada no existe.");
                if (caja.EstadoCaja != "cerrada")
                    throw new InvalidOperationException("La caja ya está abierta o en mantenimiento.");

                if (montoInicial < 0)
                    throw new ArgumentException("El monto inicial no puede ser negativo.");

                var apertura = new AperturaCaja
                {
                    IdCaja = idCaja,
                    IdCajero = idCajero,
                    MontoInicial = montoInicial,
                    FechaApertura = _fechas.ObtenerFechaActual(),
                    Observaciones = observaciones,
                    Estado = true
                };
                ctx.AperturasCaja.Add(apertura);

                caja.EstadoCaja = "abierta";

                ctx.SaveChanges();

                RegistrarAuditoria(ctx, "INSERT", "Apertura_Caja", apertura.IdApertura,
                    null, JsonConvert.SerializeObject(new { id_caja = idCaja, monto_inicial = montoInicial }),
                    $"Apertura de caja #{idCaja} por cajero #{idCajero}",
                    idUsuarioAdmin, ip, dispositivo);

                return apertura;
            }
        }

        public CierreCaja CerrarCaja(int idApertura, int idCajero, decimal saldoReal, int idUsuarioAdmin, string ip = null, string dispositivo = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var apertura = ctx.AperturasCaja.Include(a => a.Caja).FirstOrDefault(a => a.IdApertura == idApertura && a.Estado);
                if (apertura == null)
                    throw new KeyNotFoundException("La apertura especificada no existe.");
                if (apertura.Caja.EstadoCaja != "abierta")
                    throw new InvalidOperationException("La caja no está abierta.");

                if (ctx.CierresCaja.Any(c => c.IdApertura == idApertura && c.Estado))
                    throw new InvalidOperationException("Esta apertura ya tiene un cierre registrado.");

                var ventas = ctx.Ventas.Where(v => v.IdApertura == idApertura && v.EstadoVenta == "completada" && v.Estado).ToList();
                var egresos = ctx.EgresosCaja.Where(e => e.IdApertura == idApertura && e.Estado).ToList();

                var totalEfectivo = ventas.Where(v => v.MetodoPago == "efectivo").Sum(v => v.TotalCobrado);
                var totalSinpe = ventas.Where(v => v.MetodoPago == "sinpe").Sum(v => v.TotalCobrado);
                var totalTarjeta = ventas.Where(v => v.MetodoPago == "tarjeta" || v.MetodoPago == "mixto").Sum(v => v.TotalCobrado);
                var totalEgresos = egresos.Sum(e => e.Monto);
                var saldoEsperado = apertura.MontoInicial + totalEfectivo + totalSinpe + totalTarjeta - totalEgresos;

                var descuadre = saldoEsperado != saldoReal;

                var cierre = new CierreCaja
                {
                    IdCajero = idCajero,
                    IdApertura = idApertura,
                    FechaCierre = _fechas.ObtenerFechaActual(),
                    MontoApertura = apertura.MontoInicial,
                    TotalEfectivo = totalEfectivo,
                    TotalSinpe = totalSinpe,
                    TotalTarjeta = totalTarjeta,
                    TotalEgresos = totalEgresos,
                    SaldoEsperado = saldoEsperado,
                    SaldoReal = saldoReal,
                    Descuadre = descuadre,
                    Estado = true
                };
                ctx.CierresCaja.Add(cierre);

                apertura.Caja.EstadoCaja = "cerrada";

                ctx.SaveChanges();

                RegistrarAuditoria(ctx, "INSERT", "Cierres_Caja", cierre.IdCierre,
                    null, JsonConvert.SerializeObject(new { id_apertura = idApertura, saldo_esperado = saldoEsperado, saldo_real = saldoReal }),
                    descuadre ? $"Cierre con DESCUADRE: esperado {saldoEsperado:C}, real {saldoReal:C}" : "Cierre sin novedades",
                    idUsuarioAdmin, ip, dispositivo);

                return cierre;
            }
        }

        public EgresoCaja RegistrarEgreso(int idApertura, int idUsuario, string categoriaGasto, string descripcion, decimal monto, string ip = null, string dispositivo = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var apertura = ctx.AperturasCaja.Include(a => a.Caja).FirstOrDefault(a => a.IdApertura == idApertura && a.Estado);
                if (apertura == null)
                    throw new KeyNotFoundException("La apertura especificada no existe.");
                if (apertura.Caja.EstadoCaja != "abierta")
                    throw new InvalidOperationException("La caja no está abierta, no se pueden registrar egresos.");

                if (monto <= 0)
                    throw new ArgumentException("El monto del egreso debe ser mayor a cero.");

                if (string.IsNullOrWhiteSpace(categoriaGasto))
                    throw new ArgumentException("Debe especificar una categoría de gasto.");

                var egreso = new EgresoCaja
                {
                    IdApertura = idApertura,
                    IdUsuario = idUsuario,
                    CategoriaGasto = categoriaGasto,
                    Descripcion = descripcion,
                    Monto = monto,
                    FechaHora = _fechas.ObtenerFechaActual(),
                    Estado = true
                };
                ctx.EgresosCaja.Add(egreso);

                ctx.SaveChanges();

                RegistrarAuditoria(ctx, "INSERT", "Egresos_Caja", egreso.IdEgreso,
                    null, JsonConvert.SerializeObject(new { categoria = categoriaGasto, monto }),
                    $"Egreso: {categoriaGasto} - {descripcion}",
                    idUsuario, ip, dispositivo);

                return egreso;
            }
        }

        public NotaCredito AnularVenta(int idVenta, int idUsuario, string motivo, string ip = null, string dispositivo = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var venta = ctx.Ventas.FirstOrDefault(v => v.IdVenta == idVenta && v.Estado);
                if (venta == null)
                    throw new KeyNotFoundException("La venta especificada no existe.");
                if (venta.EstadoVenta != "completada")
                    throw new InvalidOperationException("Solo se pueden anular ventas en estado completada.");

                if (string.IsNullOrWhiteSpace(motivo))
                    throw new ArgumentException("Debe especificar el motivo de la anulación.");

                var valorAnterior = JsonConvert.SerializeObject(new { venta.EstadoVenta });

                venta.EstadoVenta = "anulada";

                var nota = new NotaCredito
                {
                    IdVenta = idVenta,
                    IdUsuario = idUsuario,
                    Motivo = motivo,
                    Monto = venta.TotalCobrado,
                    FechaHora = _fechas.ObtenerFechaActual(),
                    Estado = true
                };
                ctx.NotasCredito.Add(nota);

                ctx.SaveChanges();

                RegistrarAuditoria(ctx, "ANULACION", "Ventas", idVenta,
                    valorAnterior, JsonConvert.SerializeObject(new { estado_nuevo = "anulada", motivo }),
                    $"Venta #{idVenta} anulada: {motivo}",
                    idUsuario, ip, dispositivo);

                return nota;
            }
        }

        public AperturaCaja ObtenerAperturaActiva(int idCaja)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.AperturasCaja
                    .Include(a => a.Caja)
                    .Include(a => a.Cajero)
                    .FirstOrDefault(a => a.IdCaja == idCaja && a.Estado && a.Caja.EstadoCaja == "abierta");
            }
        }

        public List<Caja> ListarCajas()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Cajas.Where(c => c.Estado).OrderBy(c => c.NombreCaja).ToList();
            }
        }

        public List<AperturaCaja> ListarAperturas(int? idCaja = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.AperturasCaja.Include(a => a.Caja).Include(a => a.Cajero).Where(a => a.Estado);
                if (idCaja.HasValue)
                    query = query.Where(a => a.IdCaja == idCaja.Value);
                return query.OrderByDescending(a => a.FechaApertura).ToList();
            }
        }

        public List<CierreCaja> ListarCierres(int? idApertura = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.CierresCaja.Include(c => c.Apertura).Include(c => c.Cajero).Where(c => c.Estado);
                if (idApertura.HasValue)
                    query = query.Where(c => c.IdApertura == idApertura.Value);
                return query.OrderByDescending(c => c.FechaCierre).ToList();
            }
        }

        public List<EgresoCaja> ListarEgresos(int idApertura)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.EgresosCaja
                    .Include(e => e.Usuario)
                    .Where(e => e.IdApertura == idApertura && e.Estado)
                    .OrderByDescending(e => e.FechaHora)
                    .ToList();
            }
        }

        public List<NotaCredito> ListarNotasCredito(int? idVenta = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var query = ctx.NotasCredito.Include(n => n.Venta).Include(n => n.Usuario).Where(n => n.Estado);
                if (idVenta.HasValue)
                    query = query.Where(n => n.IdVenta == idVenta.Value);
                return query.OrderByDescending(n => n.FechaHora).ToList();
            }
        }

        public List<ReporteGenerado> ListarReportes()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.ReportesGenerados.Include(r => r.Usuario)
                    .Where(r => r.Estado)
                    .OrderByDescending(r => r.FechaGeneracion)
                    .ToList();
            }
        }

        public ReporteGenerado GenerarReporte(int idUsuario, string tipoReporte, string formato, string parametros = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var tiposValidos = new[] { "ventas", "productos_mas_vendidos", "ingresos_metodo_pago", "desempenio_meseros", "inventario", "egresos", "cierre_turno", "bitacora" };
                if (!tiposValidos.Contains(tipoReporte))
                    throw new ArgumentException($"Tipo de reporte inválido. Válidos: {string.Join(", ", tiposValidos)}");

                if (formato != "csv")
                    throw new ArgumentException("El único formato disponible es CSV.");

                var reporte = new ReporteGenerado
                {
                    IdUsuario = idUsuario,
                    TipoReporte = tipoReporte,
                    Parametros = parametros,
                    FormatoSalida = formato,
                    FechaGeneracion = _fechas.ObtenerFechaActual(),
                    Estado = true
                };
                ctx.ReportesGenerados.Add(reporte);
                ctx.SaveChanges();

                return reporte;
            }
        }

        private void RegistrarAuditoria(ColibriDbContext ctx, string accion, string tabla,
            int idRegistroAfectado, string valorAnterior, string valorNuevo,
            string detalle, int idUsuario, string ip, string dispositivo)
        {
            var bitacora = new BitacoraFinanciera
            {
                IdUsuario = idUsuario,
                TablaAfectada = tabla,
                IdRegistroAfectado = idRegistroAfectado,
                Accion = accion,
                ValorAnterior = valorAnterior,
                ValorNuevo = valorNuevo,
                Detalle = detalle,
                IpOrigen = ip,
                Dispositivo = dispositivo,
                FechaHora = _fechas.ObtenerFechaActual()
            };
            ctx.BitacoraFinanciera.Add(bitacora);
        }
    }
}
