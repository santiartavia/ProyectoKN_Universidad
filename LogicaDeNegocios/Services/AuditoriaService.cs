using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace LogicaDeNegocios.Services
{
    public class AuditoriaService : IAuditoriaService
    {
        private readonly IAuditoriaAD _auditoriaAD;
        private readonly IFechasLN _fechas;

        // Inyección de la capa de Acceso a Datos
        public AuditoriaService(IAuditoriaAD auditoriaAD, IFechasLN fechas)
        {
            _auditoriaAD = auditoriaAD;
            _fechas = fechas;
        }

        public void Registrar(string tablaAfectada, int idRegistroAfectado, string accion,
                               string valorAnterior, string valorNuevo, string detalle,
                               int idUsuario, string ipOrigen = null, string dispositivo = null)
        {
            var registro = new BitacoraRRHH
            {
                IdUsuario = idUsuario,
                TablaAfectada = tablaAfectada,
                IdRegistroAfectado = idRegistroAfectado,
                Accion = accion,
                ValorAnterior = valorAnterior,
                ValorNuevo = valorNuevo,
                Detalle = detalle,
                IpOrigen = ipOrigen,
                Dispositivo = dispositivo,
                FechaHora = _fechas.ObtenerFechaActual()
            };

            // Mandamos a guardar a la capa AD
            _auditoriaAD.Registrar(registro);
        }

        public List<BitacoraRRHH> Consultar(int? idRegistroAfectado = null, int? idUsuario = null,
                                              DateTime? fechaInicio = null, DateTime? fechaFin = null,
                                              string accion = null)
        {
            // Pasa los parámetros de búsqueda a la capa AD
            return _auditoriaAD.Consultar(idRegistroAfectado, idUsuario, fechaInicio, fechaFin, accion);
        }

        public List<string> ObtenerAccionesDistinct()
        {
            return _auditoriaAD.ObtenerAccionesDistinct();
        }
    }
}