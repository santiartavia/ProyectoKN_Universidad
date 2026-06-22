using Abstracciones.Models;
using System;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    // Interfaz para Acceso a Datos
    public interface IAuditoriaAD
    {
        void Registrar(BitacoraRRHH registro);
        List<BitacoraRRHH> Consultar(int? idRegistroAfectado = null, int? idUsuario = null,
                                      DateTime? fechaInicio = null, DateTime? fechaFin = null,
                                      string accion = null);
        List<string> ObtenerAccionesDistinct();
    }

    // Interfaz para Lógica de Negocios
    public interface IAuditoriaService
    {
        void Registrar(string tablaAfectada, int idRegistroAfectado, string accion,
                       string valorAnterior, string valorNuevo, string detalle,
                       int idUsuario, string ipOrigen = null, string dispositivo = null);
        List<BitacoraRRHH> Consultar(int? idRegistroAfectado = null, int? idUsuario = null,
                                      DateTime? fechaInicio = null, DateTime? fechaFin = null,
                                      string accion = null);
        List<string> ObtenerAccionesDistinct();
    }
}