using Abstracciones.Interfaces;
using Abstracciones.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoADatos.Clases
{
    public class AsistenciaAD : IAsistenciaAD
    {
        public int RegistrarEntrada(AsistenciaDto asistencia)
        {
            using (var db = new ColibriDbContext())
            {
                var nuevaAsistencia = new Asistencia
                {
                    IdEmpleado = asistencia.IdEmpleado,
                    FechaHoraEntrada = asistencia.FechaHoraEntrada,
                    // IdTurno = asistencia.IdTurno, // En caso de vincularlo directamente
                    Estado = true
                };

                db.Asistencias.Add(nuevaAsistencia);
                db.SaveChanges();

                return nuevaAsistencia.IdAsistencia;
            }
        }

        public int RegistrarSalida(AsistenciaDto asistencia)
        {
            using (var db = new ColibriDbContext())
            {
                // Buscamos el registro pendiente
                var registro = db.Asistencias.FirstOrDefault(a => a.IdAsistencia == asistencia.IdAsistencia);

                if (registro != null)
                {
                    registro.FechaHoraSalida = asistencia.FechaHoraSalida;
                    // Actualizamos estado o cualquier otra lógica si fuera necesario

                    // Modificamos el estado de la entidad
                    db.Entry(registro).State = EntityState.Modified;
                    db.SaveChanges();

                    return registro.IdAsistencia;
                }

                throw new Exception("No se encontró el registro de asistencia a actualizar.");
            }
        }

        public AsistenciaDto ObtenerAsistenciaPendiente(int idEmpleado)
        {
            using (var db = new ColibriDbContext())
            {
                // Escenario 6: Jornada incompleta (Tiene entrada pero no salida)
                var pendiente = db.Asistencias
                                  .Where(a => a.IdEmpleado == idEmpleado && a.FechaHoraSalida == null && a.Estado == true)
                                  .OrderByDescending(a => a.FechaHoraEntrada)
                                  .FirstOrDefault();

                if (pendiente != null)
                {
                    return new AsistenciaDto
                    {
                        IdAsistencia = pendiente.IdAsistencia,
                        IdEmpleado = pendiente.IdEmpleado,
                        FechaHoraEntrada = pendiente.FechaHoraEntrada,
                        Estado = "Pendiente"
                    };
                }

                return null;
            }
        }
    }
}