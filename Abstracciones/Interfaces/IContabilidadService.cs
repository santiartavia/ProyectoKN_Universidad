using Abstracciones.Models;
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
        List<Caja> ListarCajas();
        List<AperturaCaja> ListarAperturas(int? idCaja = null);
        List<CierreCaja> ListarCierres(int? idApertura = null);
        List<EgresoCaja> ListarEgresos(int idApertura);
        List<NotaCredito> ListarNotasCredito(int? idVenta = null);
        List<ReporteGenerado> ListarReportes();
        ReporteGenerado GenerarReporte(int idUsuario, string tipoReporte, string formato, string parametros = null);
    }
}
