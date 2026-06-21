using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogicaDeNegocios.Services
{
    public class EmpleadoService : IEmpleadoService
    {
        private readonly IAuditoriaService _auditoria;
        private readonly IFechasLN _fechas;

        public EmpleadoService(IAuditoriaService auditoria, IFechasLN fechas)
        {
            _auditoria = auditoria;
            _fechas = fechas;
        }

        public Empleado Registrar(string cedula, string nombre, string apellidos, string telefono, string correo,
                                   string rolNombre, decimal salarioHora, DateTime fechaIngreso, int idUsuarioAdmin)
        {
            if (string.IsNullOrWhiteSpace(cedula))
                throw new ArgumentException("La cédula es obligatoria");
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre es obligatorio");
            if (string.IsNullOrWhiteSpace(apellidos))
                throw new ArgumentException("Los apellidos son obligatorios");
            if (salarioHora <= 0)
                throw new ArgumentException("El salario por hora debe ser mayor a 0");
            if (!string.IsNullOrWhiteSpace(correo) && !Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("El formato del correo electrónico no es válido");
            if (fechaIngreso > _fechas.ObtenerFechaActual())
                throw new ArgumentException("La fecha de ingreso no puede ser posterior a la fecha actual");

            using (var ctx = new ColibriDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                if (ctx.Empleados.Any(e => e.Cedula == cedula && e.Estado))
                    throw new InvalidOperationException("La identificación del empleado ya existe en el sistema");

                var rol = ctx.Roles.FirstOrDefault(r => r.NombreRol == rolNombre && r.Estado);
                if (rol == null)
                    throw new ArgumentException($"El rol '{rolNombre}' no existe en el sistema");

                var usuario = new Usuario
                {
                    IdRol = rol.IdRol,
                    NombreUsuario = cedula,
                    Correo = correo ?? $"{cedula}@colibri.com",
                    PasswordHash = "",
                    CambioPasswordRequerido = true,
                    IntentosFallidos = 0,
                    Bloqueado = false,
                    Estado = true,
                    FechaCreacion = _fechas.ObtenerFechaActual()
                };
                ctx.Usuarios.Add(usuario);
                ctx.SaveChanges();

                var empleado = new Empleado
                {
                    IdUsuario = usuario.IdUsuario,
                    Cedula = cedula,
                    Nombre = nombre,
                    Apellidos = apellidos,
                    Telefono = telefono,
                    CorreoPersonal = correo,
                    SalarioHora = salarioHora,
                    DiasVacacionesDisponibles = 0,
                    FechaIngreso = fechaIngreso,
                    Estado = true
                };
                ctx.Empleados.Add(empleado);
                ctx.SaveChanges();

                tx.Commit();

                _auditoria.Registrar("Empleados", empleado.IdEmpleado, "INSERT",
                    null, Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        empleado.Cedula, empleado.Nombre, empleado.Apellidos,
                        empleado.Telefono, empleado.CorreoPersonal, empleado.SalarioHora,
                        Rol = rolNombre, empleado.FechaIngreso
                    }),
                    $"Contratación de {nombre} {apellidos} (Cédula: {cedula})",
                    idUsuarioAdmin);

                return empleado;
            }
        }

        public Empleado ObtenerPorId(int idEmpleado)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Empleados.Include(e => e.Usuario).Include(e => e.Usuario.Rol)
                    .FirstOrDefault(e => e.IdEmpleado == idEmpleado);
            }
        }

        public Empleado ObtenerPorCedula(string cedula)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Empleados.Include(e => e.Usuario).Include(e => e.Usuario.Rol)
                    .FirstOrDefault(e => e.Cedula == cedula);
            }
        }

        public List<Empleado> ListarActivos()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Empleados.Include(e => e.Usuario).Include(e => e.Usuario.Rol)
                    .Where(e => e.Estado).ToList();
            }
        }

        public List<Empleado> ListarTodos()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Empleados.Include(e => e.Usuario).Include(e => e.Usuario.Rol).ToList();
            }
        }

        public Empleado Actualizar(int idEmpleado, string telefono, string correo, string rolNombre, decimal salarioHora, int idUsuarioAdmin)
        {
            if (salarioHora <= 0)
                throw new ArgumentException("El salario por hora debe ser mayor a 0");
            if (!string.IsNullOrWhiteSpace(correo) && !Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("El formato del correo electrónico no es válido");

            using (var ctx = new ColibriDbContext())
            {
                var empleado = ctx.Empleados.Include(e => e.Usuario).Include(e => e.Usuario.Rol)
                    .FirstOrDefault(e => e.IdEmpleado == idEmpleado);
                if (empleado == null)
                    throw new KeyNotFoundException("Empleado no encontrado");

                var valorAnterior = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    empleado.Telefono, empleado.CorreoPersonal, empleado.SalarioHora,
                    Rol = empleado.Usuario?.Rol?.NombreRol
                });

                empleado.Telefono = telefono ?? empleado.Telefono;
                empleado.CorreoPersonal = correo ?? empleado.CorreoPersonal;
                empleado.SalarioHora = salarioHora;
                empleado.FechaModificacion = _fechas.ObtenerFechaActual();

                if (!string.IsNullOrWhiteSpace(rolNombre))
                {
                    var rol = ctx.Roles.FirstOrDefault(r => r.NombreRol == rolNombre && r.Estado);
                    if (rol == null)
                        throw new ArgumentException($"El rol '{rolNombre}' no existe");
                    empleado.Usuario.IdRol = rol.IdRol;
                }

                ctx.SaveChanges();

                _auditoria.Registrar("Empleados", empleado.IdEmpleado, "UPDATE",
                    valorAnterior,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        telefono, correo, salarioHora, Rol = rolNombre
                    }),
                    $"Modificación de datos contractuales de {empleado.Nombre} {empleado.Apellidos}",
                    idUsuarioAdmin);

                return empleado;
            }
        }

        public Empleado Inactivar(int idEmpleado, string motivo, int idUsuarioAdmin)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo de inactivación es obligatorio");

            using (var ctx = new ColibriDbContext())
            {
                var empleado = ctx.Empleados.Include(e => e.Usuario).FirstOrDefault(e => e.IdEmpleado == idEmpleado);
                if (empleado == null)
                    throw new KeyNotFoundException("Empleado no encontrado");

                empleado.Estado = false;
                empleado.MotivoInactivacion = motivo;
                empleado.FechaModificacion = _fechas.ObtenerFechaActual();
                if (empleado.Usuario != null)
                    empleado.Usuario.Estado = false;
                ctx.SaveChanges();

                _auditoria.Registrar("Empleados", empleado.IdEmpleado, "DESACTIVACION",
                    "Estado: Activo",
                    $"Estado: Inactivo. Motivo: {motivo}",
                    $"Desactivación de {empleado.Nombre} {empleado.Apellidos}",
                    idUsuarioAdmin);

                return empleado;
            }
        }

        public Empleado Reactivar(int idEmpleado, int idUsuarioAdmin)
        {
            using (var ctx = new ColibriDbContext())
            {
                var empleado = ctx.Empleados.Include(e => e.Usuario).FirstOrDefault(e => e.IdEmpleado == idEmpleado);
                if (empleado == null)
                    throw new KeyNotFoundException("Empleado no encontrado");

                empleado.Estado = true;
                empleado.FechaReactivacion = _fechas.ObtenerFechaActual();
                empleado.MotivoInactivacion = null;
                empleado.FechaModificacion = _fechas.ObtenerFechaActual();
                if (empleado.Usuario != null)
                    empleado.Usuario.Estado = true;
                ctx.SaveChanges();

                _auditoria.Registrar("Empleados", empleado.IdEmpleado, "ACTIVACION",
                    "Estado: Inactivo",
                    "Estado: Activo",
                    $"Reactivación de {empleado.Nombre} {empleado.Apellidos}",
                    idUsuarioAdmin);

                return empleado;
            }
        }

        public List<Empleado> ListarInactivos()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Empleados.Include(e => e.Usuario).Include(e => e.Usuario.Rol)
                    .Where(e => !e.Estado)
                    .ToList();
            }
        }

        public Empleado ObtenerPorUsuarioId(int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Empleados.Include(e => e.Usuario).Include(e => e.Usuario.Rol)
                    .FirstOrDefault(e => e.IdUsuario == idUsuario && e.Estado);
            }
        }

        public List<Empleado> Buscar(string termino)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Empleados.Include(e => e.Usuario).Include(e => e.Usuario.Rol)
                    .Where(e => e.Estado && (e.Cedula.Contains(termino) ||
                                              e.Nombre.Contains(termino) ||
                                              e.Apellidos.Contains(termino) ||
                                              (e.Nombre + " " + e.Apellidos).Contains(termino)))
                    .ToList();
            }
        }
    }
}