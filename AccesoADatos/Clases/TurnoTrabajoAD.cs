using Abstracciones.Interfaces;
using Abstracciones.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoADatos.Clases
{
    public class TurnoTrabajoAD : ITurnoTrabajoAD
    {
        public int CrearTurno(TurnoTrabajoDto turno)
        {
            using (var db = new ColibriDbContext())
            {
                var nuevoTurno = new TurnoTrabajo
                {
                    IdEmpleado = turno.IdEmpleado,
                    FechaTurno = turno.FechaTurno,
                    HoraInicio = turno.HoraInicio,
                    HoraFin = turno.HoraFin,

                    // Nota técnica: Según el OnModelCreating que pasaste, TurnoTrabajo 
                    // mapea "Descripcion" en lugar de "TipoTurno" o "JornadaHoras". 
                    // Si agregas esas propiedades a la entidad más adelante, las puedes descomentar:
                    // TipoTurno = turno.TipoTurno,
                    // JornadaHoras = turno.JornadaHoras,

                    Estado = true
                };

                db.TurnosTrabajo.Add(nuevoTurno);
                db.SaveChanges();

                return nuevoTurno.IdTurno; // Retorno del ID autogenerado para redirigir
            }
        }

        public bool ExisteTurno(int idEmpleado, DateTime fechaTurno)
        {
            using (var db = new ColibriDbContext())
            {
                // Verifica duplicidad consultando el DbSet TurnosTrabajo
                return db.TurnosTrabajo.Any(t => t.IdEmpleado == idEmpleado
                                              && t.FechaTurno == fechaTurno
                                              && t.Estado == true);
            }
        }
    }
}