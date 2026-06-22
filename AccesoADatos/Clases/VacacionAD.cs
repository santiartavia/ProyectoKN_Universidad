using Abstracciones.Interfaces;
using Abstracciones.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoADatos.Clases
{
    public class VacacionAD : IVacacionAD
    {
        private void RegistrarAuditoria(ColibriDbContext db, int idUsuario, string tabla, int idRegistro, string accion, string valAnt, string valNue, string detalle)
        {
            db.BitacoraRRHH.Add(new BitacoraRRHH
            {
                IdUsuario = idUsuario,
                TablaAfectada = tabla,
                IdRegistroAfectado = idRegistro,
                Accion = accion,
                ValorAnterior = valAnt,
                ValorNuevo = valNue,
                Detalle = detalle,
                FechaHora = DateTime.Now
            });
        }

        public decimal ObtenerSaldoVacaciones(int idEmpleado)
        {
            using (var db = new ColibriDbContext())
            {
                var emp = db.Empleados.Find(idEmpleado);
                return emp != null ? (decimal)emp.DiasVacacionesDisponibles : 0;
            }
        }

        public void ActualizarSaldoVacaciones(int idEmpleado, decimal nuevoSaldo, int idUsuarioResponsable)
        {
            using (var db = new ColibriDbContext())
            {
                var empleado = db.Empleados.Find(idEmpleado);
                if (empleado == null) throw new Exception("Empleado no encontrado.");

                string saldoAnterior = empleado.DiasVacacionesDisponibles.ToString();
                empleado.DiasVacacionesDisponibles = nuevoSaldo;

                RegistrarAuditoria(db, idUsuarioResponsable, "Empleados", idEmpleado, "Cálculo Vacaciones", saldoAnterior, nuevoSaldo.ToString(), "Actualización automática de saldo de vacaciones.");

                db.SaveChanges();
            }
        }

        public int CrearSolicitud(Vacacion solicitud, int idUsuarioResponsable)
        {
            using (var db = new ColibriDbContext())
            {
                solicitud.EstadoSolicitud = "Solicitada";
                solicitud.FechaSolicitud = DateTime.Now;
                solicitud.Estado = true;

                db.Vacaciones.Add(solicitud);
                db.SaveChanges(); // Guardamos para obtener el ID generado

                RegistrarAuditoria(db, idUsuarioResponsable, "Vacaciones", solicitud.IdVacacion, "Creación", "", "Solicitada", $"Solicitud creada por {solicitud.DiasSolicitados} días.");
                db.SaveChanges();

                return solicitud.IdVacacion;
            }
        }

        public void AprobarSolicitud(int idVacacion, int idAprobador, int idUsuarioResponsable)
        {
            using (var db = new ColibriDbContext())
            {
                var vacacion = db.Vacaciones.Find(idVacacion);
                if (vacacion == null) throw new Exception("Solicitud no encontrada.");

                var empleado = db.Empleados.Find(vacacion.IdEmpleado);

                vacacion.EstadoSolicitud = "Aprobada";
                vacacion.IdAprobador = idAprobador;

                decimal saldoAnterior = (decimal)empleado.DiasVacacionesDisponibles;
                empleado.DiasVacacionesDisponibles -= vacacion.DiasSolicitados;

                RegistrarAuditoria(db, idUsuarioResponsable, "Vacaciones", idVacacion, "Aprobación", "Solicitada", "Aprobada", $"Vacaciones aprobadas. Saldo anterior: {saldoAnterior}, Nuevo Saldo: {empleado.DiasVacacionesDisponibles}");

                db.SaveChanges();
            }
        }

        public void RechazarSolicitud(int idVacacion, string motivoRechazo, int idUsuarioResponsable)
        {
            using (var db = new ColibriDbContext())
            {
                var vacacion = db.Vacaciones.Find(idVacacion);
                if (vacacion == null) throw new Exception("Solicitud no encontrada.");

                vacacion.EstadoSolicitud = "Rechazada";
                vacacion.MotivoRechazo = motivoRechazo;

                RegistrarAuditoria(db, idUsuarioResponsable, "Vacaciones", idVacacion, "Rechazo", "Solicitada", "Rechazada", $"Motivo: {motivoRechazo}");

                db.SaveChanges();
            }
        }

        public bool ExistenTurnosActivos(int idEmpleado, DateTime fechaInicio, DateTime fechaFin)
        {
            using (var db = new ColibriDbContext())
            {
                return db.TurnosTrabajo.Any(t => t.IdEmpleado == idEmpleado
                                              && t.Estado == true
                                              && t.FechaTurno >= fechaInicio
                                              && t.FechaTurno <= fechaFin);
            }
        }
    }
}