using Abstracciones.Models;
using System;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface IContabilidadService
    {
        AperturaCaja AbrirCaja(int idCaja, int idCajero, decimal montoInicial, string observaciones, int idUsuarioAdmin, string ip = null, string dispositivo = null);
        CierreCaja CerrarCaja(int idApertura, int idCajero, decimal saldoReal, int idUsuarioAdmin, string ip = null, string dispositivo = null);
        EgresoCaja RegistrarEgreso(int idApertura, int idUsuario, string categoriaGasto, string descripcion, decimal monto, string ip = null, string dispositivo = null);
        NotaCredito AnularVenta(int idVenta, int idUsuario, string motivo, string ip = null, string dispositivo = null);
        AperturaCaja ObtenerAperturaActiva(int idCaja);
        bool TieneAperturaActiva(int idCajero);
        List<Caja> ListarCajas();
        List<AperturaCaja> ListarAperturas(int? idCaja = null);
        List<CierreCaja> ListarCierres(int? idApertura = null);
        List<EgresoCaja> ListarEgresos(int idApertura);
        decimal ObtenerSaldoDisponible(int idApertura);
        List<NotaCredito> ListarNotasCredito(int? idVenta = null);
        bool TieneNotaCredito(int idVenta);
        List<ReporteGenerado> ListarReportes();
        ReporteGenerado GenerarReporte(int idUsuario, string tipoReporte, string formato, string parametros = null);
        CierrePeriodo GenerarCierrePeriodo(int idUsuario, string tipoPeriodo, int? mes, int anio);
        List<CierrePeriodo> ListarCierresPeriodo();
        List<BitacoraFinanciera> ConsultarBitacoraFinanciera(int? idUsuario = null, string accion = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        List<string> ObtenerAccionesBitacoraFinanciera();
        void ExportarBitacoraFinancieraCsv(int? idUsuario, string accion, DateTime? fechaInicio, DateTime? fechaFin);
        List<BitacoraReporte> ConsultarBitacoraReportes(int? idUsuario = null, string accion = null, DateTime? fechaInicio = null, DateTime? fechaFin = null);
        List<string> ObtenerAccionesBitacoraReportes();
        BitacoraReporte RegistrarBitacoraReporte(int idUsuario, string accion, string detalle, string valorNuevo = null, string ip = null, string dispositivo = null);
    }
}
