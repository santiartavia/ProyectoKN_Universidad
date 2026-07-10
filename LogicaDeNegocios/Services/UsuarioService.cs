using Abstracciones.Interfaces;
using Abstracciones.Models;
using AccesoADatos;
using LogicaDeNegocios.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogicaDeNegocios.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IFechasLN _fechas;
        private const int DIAS_VIGENCIA_PASSWORD = 180; // 6 meses
        private const int DIAS_AVISO_PREVIO = 15;
        private const int MAX_INTENTOS_FALLIDOS = 5;
        private const int HISTORIAL_PASSWORDS = 5;

        public UsuarioService(IFechasLN fechas)
        {
            _fechas = fechas;
        }

        private int ObtenerIdSesionActiva(ColibriDbContext ctx, int idUsuario)
        {
            var sesion = ctx.Sesiones
                .FirstOrDefault(s => s.IdUsuario == idUsuario && s.EstadoSesion == "ACTIVA");
            return sesion?.IdSesion ?? 0;
        }

        private void RegistrarAuditoria(ColibriDbContext ctx, string accion, string tabla,
            int idRegistroAfectado, string valorAnterior, string valorNuevo,
            string detalle, int idUsuario, string ip, string dispositivo)
        {
            var bitacora = new BitacoraRRHH
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
            ctx.BitacoraRRHH.Add(bitacora);
        }

        private void RegistrarBitacoraAcceso(ColibriDbContext ctx, int idUsuario, string accion,
            string detalle, string ip, string dispositivo)
        {
            var reg = new BitacoraAcceso
            {
                IdUsuario = idUsuario,
                Accion = accion,
                Detalle = detalle,
                IpOrigen = ip,
                Dispositivo = dispositivo,
                FechaHora = _fechas.ObtenerFechaActual()
            };
            ctx.BitacoraAcceso.Add(reg);
        }

        private void GuardarHistorialPassword(ColibriDbContext ctx, int idUsuario, string passwordHash,
            string metodo, string ip, string dispositivo)
        {
            var historial = new PasswordHistorial
            {
                IdUsuario = idUsuario,
                PasswordHash = passwordHash,
                FechaCambio = _fechas.ObtenerFechaActual(),
                MetodoCambio = metodo,
                Dispositivo = dispositivo,
                DireccionIp = ip
            };
            ctx.PasswordHistorial.Add(historial);
        }

        private bool PasswordUsadaAnteriormente(ColibriDbContext ctx, int idUsuario, string nuevaPassword)
        {
            var historiales = ctx.PasswordHistorial
                .Where(h => h.IdUsuario == idUsuario)
                .OrderByDescending(h => h.FechaCambio)
                .Take(HISTORIAL_PASSWORDS)
                .ToList();

            foreach (var h in historiales)
            {
                try
                {
                    if (PasswordHelper.VerifyPassword(nuevaPassword, h.PasswordHash))
                        return true;
                }
                catch { }
            }
            return false;
        }

        private int ContarHistorial(ColibriDbContext ctx, int idUsuario)
        {
            return ctx.PasswordHistorial.Count(h => h.IdUsuario == idUsuario);
        }

        // GUS 001: Crear usuario
        public Usuario CrearUsuario(string nombreUsuario, string correo, string password, int idRol,
            int idUsuarioAdmin, string ip = null, string dispositivo = null)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                throw new ArgumentException("Faltan campos requeridos: nombre de usuario.");
            if (string.IsNullOrWhiteSpace(correo))
                throw new ArgumentException("Faltan campos requeridos: correo.");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Faltan campos requeridos: contraseña.");
            if (idRol <= 0)
                throw new ArgumentException("Faltan campos requeridos: rol.");

            if (PasswordHelper.ContieneCaracteresInvalidos(nombreUsuario, out string msgNombre))
                throw new ArgumentException($"El atributo nombre de usuario no corresponde al tipo de dato: {msgNombre}");
            if (PasswordHelper.ContieneCaracteresInvalidos(correo, out string msgCorreo))
                throw new ArgumentException($"El atributo correo no corresponde al tipo de dato: {msgCorreo}");

            if (!PasswordHelper.EsFormatoCorreoValido(correo))
                throw new ArgumentException("El formato del correo no es válido.");

            if (!PasswordHelper.CumplePoliticaSeguridad(password, out string msgPassword))
                throw new ArgumentException($"No se cumple con los caracteres o tamaño requerido para la contraseña: {msgPassword}");

            using (var ctx = new ColibriDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                if (ctx.Usuarios.Any(u => u.NombreUsuario == nombreUsuario && u.Estado))
                    throw new InvalidOperationException("El usuario ya existe.");

                if (ctx.Usuarios.Any(u => u.Correo == correo && u.Estado))
                    throw new InvalidOperationException("El correo ya está registrado.");

                var rol = ctx.Roles.Find(idRol);
                if (rol == null || !rol.Estado)
                    throw new ArgumentException("El rol especificado no existe.");

                var passwordHash = PasswordHelper.HashPassword(password);

                var usuario = new Usuario
                {
                    IdRol = idRol,
                    NombreUsuario = nombreUsuario,
                    Correo = correo,
                    PasswordHash = passwordHash,
                    CambioPasswordRequerido = false,
                    IntentosFallidos = 0,
                    Bloqueado = false,
                    Estado = true,
                    FechaCreacion = _fechas.ObtenerFechaActual(),
                    FechaPassword = _fechas.ObtenerFechaActual()
                };
                ctx.Usuarios.Add(usuario);
                ctx.SaveChanges();

                GuardarHistorialPassword(ctx, usuario.IdUsuario, passwordHash, "MANUAL", ip, dispositivo);

                RegistrarAuditoria(ctx, "INSERT", "Usuarios", usuario.IdUsuario,
                    null,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        usuario.NombreUsuario, usuario.Correo, Rol = rol.NombreRol,
                        usuario.Estado, FechaCreacion = usuario.FechaCreacion
                    }),
                    $"Se agregó un nuevo usuario: {nombreUsuario} con rol {rol.NombreRol}",
                    idUsuarioAdmin, ip, dispositivo);

                tx.Commit();
                return usuario;
            }
        }

        // GUS 004: Editar usuario
        public Usuario EditarUsuario(int idUsuario, string correo, string telefono, string direccion,
            int idUsuarioAdmin, string ip = null, string dispositivo = null)
        {
            using (var ctx = new ColibriDbContext())
            {
                var usuario = ctx.Usuarios.Include("Rol").FirstOrDefault(u => u.IdUsuario == idUsuario && u.Estado);
                if (usuario == null)
                    throw new KeyNotFoundException("El usuario con ese número de ID no existe, que vuelva a poner el número correctamente.");

                var valorAnterior = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    usuario.Correo
                });

                // Validar correo
                if (!string.IsNullOrWhiteSpace(correo))
                {
                    if (!PasswordHelper.EsFormatoCorreoValido(correo))
                        throw new ArgumentException("El correo es inválido. Formato correcto: usuario@dominio.com");

                    if (PasswordHelper.ContieneCaracteresInvalidos(correo, out string msgC))
                        throw new ArgumentException($"El atributo correo no corresponde al tipo de dato: {msgC}");

                    if (ctx.Usuarios.Any(u => u.Correo == correo && u.IdUsuario != idUsuario && u.Estado))
                        throw new InvalidOperationException("El correo ya está siendo usado por otro usuario.");

                    usuario.Correo = correo;

                    var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == idUsuario && e.Estado);
                    if (empleado != null)
                    {
                        empleado.CorreoPersonal = correo;
                        empleado.FechaModificacion = _fechas.ObtenerFechaActual();
                    }
                }

                if (!string.IsNullOrWhiteSpace(telefono))
                {
                    if (!Regex.IsMatch(telefono, @"^[\d\-\(\)\s\+]+$"))
                        throw new ArgumentException("El teléfono contiene caracteres no válidos.");

                    var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == idUsuario && e.Estado);
                    if (empleado != null)
                    {
                        empleado.Telefono = telefono;
                        empleado.FechaModificacion = _fechas.ObtenerFechaActual();
                    }
                }

                if (!string.IsNullOrWhiteSpace(direccion))
                {
                    if (!PasswordHelper.EsDireccionValida(direccion, out string msgDir))
                        throw new ArgumentException($"La dirección no cumple con los campos mínimos requeridos: {msgDir}");

                    usuario.Direccion = direccion;

                    var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == idUsuario && e.Estado);
                    if (empleado != null)
                    {
                        empleado.Direccion = direccion;
                        empleado.FechaModificacion = _fechas.ObtenerFechaActual();
                    }
                }

                var valorNuevo = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    Correo = correo, Telefono = telefono, Direccion = direccion
                });

                ctx.SaveChanges();

                RegistrarAuditoria(ctx, "UPDATE", "Usuarios", usuario.IdUsuario,
                    valorAnterior, valorNuevo,
                    $"Se actualizaron datos del usuario {usuario.NombreUsuario}",
                    idUsuarioAdmin, ip, dispositivo);

                return usuario;
            }
        }

        // GUS 005: Asignar rol
        public Usuario AsignarRol(int idUsuario, int idRolNuevo, int idUsuarioAdmin,
            string ip = null, string dispositivo = null)
        {
            using (var ctx = new ColibriDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                var usuario = ctx.Usuarios.Include("Rol").FirstOrDefault(u => u.IdUsuario == idUsuario && u.Estado);
                if (usuario == null)
                    throw new KeyNotFoundException("El usuario no existe.");

                if (!usuario.Estado)
                    throw new InvalidOperationException("El usuario no tiene permisos (cuenta inactiva).");

                var rolNuevo = ctx.Roles.Find(idRolNuevo);
                if (rolNuevo == null || !rolNuevo.Estado)
                    throw new ArgumentException("Rol inválido porque no está definido.");

                if (usuario.IdRol == idRolNuevo)
                    throw new InvalidOperationException("El usuario ya tiene ese rol asignado.");

                string rolAnteriorNombre = usuario.Rol?.NombreRol ?? "";

                usuario.IdRol = idRolNuevo;
                ctx.SaveChanges();

                RegistrarAuditoria(ctx, "ROLE_ASSIGNMENT", "Usuarios", usuario.IdUsuario,
                    $"Rol anterior: {rolAnteriorNombre}",
                    $"Rol nuevo: {rolNuevo.NombreRol}",
                    $"Se asignó rol {rolNuevo.NombreRol} al usuario {usuario.NombreUsuario} en fecha {_fechas.ObtenerFechaActual():dd/MM/yyyy}",
                    idUsuarioAdmin, ip, dispositivo);

                tx.Commit();
                return usuario;
            }
        }

        // GUS 006: Cambiar password (con verificación de anterior)
        public Usuario CambiarPassword(int idUsuario, string passwordActual, string passwordNuevo,
            string ip = null, string dispositivo = null)
        {
            if (string.IsNullOrWhiteSpace(passwordActual))
                throw new ArgumentException("Debe ingresar la contraseña actual.");
            if (string.IsNullOrWhiteSpace(passwordNuevo))
                throw new ArgumentException("Debe ingresar la nueva contraseña.");

            if (!PasswordHelper.CumplePoliticaSeguridad(passwordNuevo, out string msgSeg))
                throw new ArgumentException($"Su contraseña ya no es segura y deberá ingresar una nueva: {msgSeg}");

            using (var ctx = new ColibriDbContext())
            {
                var usuario = ctx.Usuarios.Find(idUsuario);
                if (usuario == null)
                    throw new KeyNotFoundException("Usuario no encontrado.");

                if (!PasswordHelper.VerifyPassword(passwordActual, usuario.PasswordHash))
                    throw new InvalidOperationException("La contraseña actual no es correcta.");

                if (PasswordHelper.VerifyPassword(passwordNuevo, usuario.PasswordHash))
                    throw new InvalidOperationException("La contraseña debe ser diferente a la anterior.");

                if (PasswordUsadaAnteriormente(ctx, idUsuario, passwordNuevo))
                    throw new InvalidOperationException("La contraseña debe ser diferente a las últimas " + HISTORIAL_PASSWORDS + " contraseñas utilizadas.");

                var nuevoHash = PasswordHelper.HashPassword(passwordNuevo);
                usuario.PasswordHash = nuevoHash;
                usuario.CambioPasswordRequerido = false;
                usuario.FechaPassword = _fechas.ObtenerFechaActual();
                usuario.UltimoCambioPasswordIp = ip;
                usuario.UltimoCambioPasswordDispositivo = dispositivo;
                ctx.SaveChanges();

                GuardarHistorialPassword(ctx, idUsuario, nuevoHash, "MANUAL", ip, dispositivo);

                RegistrarAuditoria(ctx, "PASSWORD_CHANGE", "Usuarios", idUsuario,
                    null, null,
                    $"Cambio de contraseña del usuario {usuario.NombreUsuario}",
                    idUsuario, ip, dispositivo);

                return usuario;
            }
        }

        // GUS 006: Cambio forzado de contraseña (primer ingreso / obligatorio)
        public Usuario CambiarPasswordForzado(int idUsuario, string passwordNuevo, string metodoCambio,
            string ip = null, string dispositivo = null)
        {
            if (string.IsNullOrWhiteSpace(passwordNuevo))
                throw new ArgumentException("Debe ingresar una contraseña.");

            if (!PasswordHelper.CumplePoliticaSeguridad(passwordNuevo, out string msgSeg))
                throw new ArgumentException($"Su contraseña ya no es segura y deberá ingresar una nueva: {msgSeg}");

            using (var ctx = new ColibriDbContext())
            {
                var usuario = ctx.Usuarios.Find(idUsuario);
                if (usuario == null)
                    throw new KeyNotFoundException("Usuario no encontrado.");

                if (!string.IsNullOrEmpty(usuario.PasswordHash) &&
                    ContarHistorial(ctx, idUsuario) > 0 &&
                    PasswordUsadaAnteriormente(ctx, idUsuario, passwordNuevo))
                    throw new InvalidOperationException("La contraseña debe ser diferente a las últimas " + HISTORIAL_PASSWORDS + " contraseñas utilizadas.");

                var nuevoHash = PasswordHelper.HashPassword(passwordNuevo);
                usuario.PasswordHash = nuevoHash;
                usuario.CambioPasswordRequerido = false;
                usuario.FechaPassword = _fechas.ObtenerFechaActual();
                usuario.UltimoCambioPasswordIp = ip;
                usuario.UltimoCambioPasswordDispositivo = dispositivo;
                ctx.SaveChanges();

                GuardarHistorialPassword(ctx, idUsuario, nuevoHash, metodoCambio, ip, dispositivo);

                RegistrarAuditoria(ctx, "PASSWORD_CHANGE", "Usuarios", idUsuario,
                    null, null,
                    $"Cambio de contraseña ({metodoCambio}) del usuario {usuario.NombreUsuario}",
                    idUsuario, ip, dispositivo);

                return usuario;
            }
        }

        // Verificar contraseña (para login)
        public bool VerificarPassword(int idUsuario, string password)
        {
            using (var ctx = new ColibriDbContext())
            {
                var usuario = ctx.Usuarios.Find(idUsuario);
                if (usuario == null || string.IsNullOrEmpty(usuario.PasswordHash))
                    return false;

                return PasswordHelper.VerifyPassword(password, usuario.PasswordHash);
            }
        }

        // GUS 007: Eliminar usuario
        public Usuario EliminarUsuario(int idUsuario, int idUsuarioAdmin, string ip = null, string dispositivo = null)
        {
            using (var ctx = new ColibriDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                var usuario = ctx.Usuarios.Include("Rol").FirstOrDefault(u => u.IdUsuario == idUsuario);
                if (usuario == null)
                    throw new KeyNotFoundException("Usuario no existente.");

                // No permitir eliminar administradores principales (protegidos)
                if (usuario.Rol?.NombreRol == "Administrador" && usuario.Estado)
                {
                    var adminCount = ctx.Usuarios.Count(u => u.Rol.NombreRol == "Administrador" && u.Estado);
                    if (adminCount <= 1)
                        throw new InvalidOperationException("El usuario no puede ser eliminado: es el único administrador del sistema.");
                }

                // Verificar si el admin que elimina tiene permisos
                var admin = ctx.Usuarios.Find(idUsuarioAdmin);
                if (admin == null || admin.Rol?.NombreRol != "Administrador")
                    throw new UnauthorizedAccessException("No cuenta con privilegios para eliminar usuarios.");

                // Soft delete
                string nombreUsuario = usuario.NombreUsuario;
                string rolUsuario = usuario.Rol?.NombreRol ?? "";
                usuario.Estado = false;
                usuario.Bloqueado = true;

                // Inactivar empleado asociado
                var empleado = ctx.Empleados.FirstOrDefault(e => e.IdUsuario == idUsuario && e.Estado);
                if (empleado != null)
                {
                    empleado.Estado = false;
                    empleado.MotivoInactivacion = "Usuario eliminado por administrador";
                    empleado.FechaModificacion = _fechas.ObtenerFechaActual();
                }

                ctx.SaveChanges();

                RegistrarAuditoria(ctx, "DELETE", "Usuarios", usuario.IdUsuario,
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { Nombre = nombreUsuario, Rol = rolUsuario, Estado = true }),
                    Newtonsoft.Json.JsonConvert.SerializeObject(new { Estado = false, Bloqueado = true }),
                    $"Usuario eliminado: {nombreUsuario} (Rol: {rolUsuario}) en {_fechas.ObtenerFechaActual():dd/MM/yyyy HH:mm}",
                    idUsuarioAdmin, ip, dispositivo);

                tx.Commit();
                return usuario;
            }
        }

        // GUS 007: depuración de usuarios eliminados
        public void DepurarUsuarios(List<int> idsUsuarios, int idUsuarioAdmin, string ip = null, string dispositivo = null)
        {
            using (var ctx = new ColibriDbContext())
            using (var tx = ctx.Database.BeginTransaction())
            {
                var admin = ctx.Usuarios.Find(idUsuarioAdmin);
                if (admin == null || admin.Rol?.NombreRol != "Administrador")
                    throw new UnauthorizedAccessException("No cuenta con privilegios para depurar usuarios.");

                var usuariosAEliminar = ctx.Usuarios.Where(u => idsUsuarios.Contains(u.IdUsuario) && !u.Estado).ToList();

                foreach (var u in usuariosAEliminar)
                {
                    RegistrarAuditoria(ctx, "DELETE", "Usuarios", u.IdUsuario,
                        Newtonsoft.Json.JsonConvert.SerializeObject(new { u.NombreUsuario, u.Correo }),
                        "Eliminado permanentemente",
                        $"depuración: usuario {u.NombreUsuario} eliminado permanentemente.",
                        idUsuarioAdmin, ip, dispositivo);
                    ctx.Usuarios.Remove(u);
                }

                ctx.SaveChanges();
                tx.Commit();
            }
        }

        public List<Usuario> ListarActivos()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Usuarios.Include("Rol")
                    .Where(u => u.Estado)
                    .OrderBy(u => u.IdUsuario)
                    .ToList();
            }
        }

        public List<Usuario> ListarInactivos()
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Usuarios.Include("Rol")
                    .Where(u => !u.Estado)
                    .OrderBy(u => u.IdUsuario)
                    .ToList();
            }
        }

        public Usuario ObtenerPorId(int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Usuarios.Include("Rol")
                    .FirstOrDefault(u => u.IdUsuario == idUsuario && u.Estado);
            }
        }

        public Usuario ObtenerPorNombre(string nombreUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                return ctx.Usuarios.Include("Rol")
                    .FirstOrDefault(u => u.NombreUsuario == nombreUsuario && u.Estado);
            }
        }

        public bool NombreUsuarioDisponible(string nombreUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                return !ctx.Usuarios.Any(u => u.NombreUsuario == nombreUsuario && u.Estado);
            }
        }

        public bool CorreoDisponible(string correo)
        {
            using (var ctx = new ColibriDbContext())
            {
                return !ctx.Usuarios.Any(u => u.Correo == correo && u.Estado);
            }
        }

        // Métodos auxiliares para el LoginController
        public void RegistrarIntentoFallido(int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                var user = ctx.Usuarios.Find(idUsuario);
                if (user == null) return;

                user.IntentosFallidos++;
                if (user.IntentosFallidos >= MAX_INTENTOS_FALLIDOS)
                {
                    user.Bloqueado = true;
                }
                if (user.IntentosFallidos >= 3)
                {
                    user.CambioPasswordRequerido = true;
                }
                ctx.SaveChanges();
            }
        }

        public void ResetearIntentos(int idUsuario)
        {
            using (var ctx = new ColibriDbContext())
            {
                var user = ctx.Usuarios.Find(idUsuario);
                if (user == null) return;
                user.IntentosFallidos = 0;
                user.FechaUltimoAcceso = _fechas.ObtenerFechaActual();
                ctx.SaveChanges();
            }
        }

        public bool PasswordExpirada(Usuario usuario)
        {
            if (!usuario.FechaPassword.HasValue) return true;
            var fechaActual = _fechas.ObtenerFechaActual();
            return (fechaActual - usuario.FechaPassword.Value).TotalDays >= DIAS_VIGENCIA_PASSWORD;
        }

        public bool PasswordProximaAVencer(Usuario usuario)
        {
            if (!usuario.FechaPassword.HasValue) return true;
            var fechaActual = _fechas.ObtenerFechaActual();
            var diasUsados = (fechaActual - usuario.FechaPassword.Value).TotalDays;
            return diasUsados >= (DIAS_VIGENCIA_PASSWORD - DIAS_AVISO_PREVIO);
        }

        public int DiasRestantesPassword(Usuario usuario)
        {
            if (!usuario.FechaPassword.HasValue) return 0;
            var fechaActual = _fechas.ObtenerFechaActual();
            var diasUsados = (int)(fechaActual - usuario.FechaPassword.Value).TotalDays;
            return Math.Max(0, DIAS_VIGENCIA_PASSWORD - diasUsados);
        }

        // Crear sesión
        public Sesion CrearSesion(int idUsuario, string ip, string dispositivo)
        {
            using (var ctx = new ColibriDbContext())
            {
                var sesion = new Sesion
                {
                    IdUsuario = idUsuario,
                    EstadoSesion = "ACTIVA",
                    FechaHoraInicio = _fechas.ObtenerFechaActual(),
                    FechaHoraUltimaAct = _fechas.ObtenerFechaActual(),
                    DispositivoAcceso = dispositivo,
                    DireccionIp = ip
                };
                ctx.Sesiones.Add(sesion);
                ctx.SaveChanges();

                RegistrarBitacoraAcceso(ctx, idUsuario, "LOGIN_SUCCESS",
                    $"Inicio de sesión correcto en {_fechas.ObtenerFechaActual():dd/MM/yyyy HH:mm}",
                    ip, dispositivo);

                return sesion;
            }
        }

        // Cerrar sesión
        public void CerrarSesion(int idSesion, int idUsuario, string motivo, string ip, string dispositivo)
        {
            using (var ctx = new ColibriDbContext())
            {
                var sesion = ctx.Sesiones.Find(idSesion);
                if (sesion != null && sesion.EstadoSesion == "ACTIVA")
                {
                    sesion.EstadoSesion = motivo == "EXPIRADA" ? "EXPIRADA" : "CERRADA";
                    sesion.FechaHoraCierre = _fechas.ObtenerFechaActual();
                    sesion.MotivoCierre = motivo;
                }

                RegistrarBitacoraAcceso(ctx, idUsuario, "LOGOUT",
                    $"Cierre de sesión ({motivo}) en {_fechas.ObtenerFechaActual():dd/MM/yyyy HH:mm}",
                    ip, dispositivo);

                ctx.SaveChanges();
            }
        }

        public void ActualizarUltimaActividad(int idSesion)
        {
            using (var ctx = new ColibriDbContext())
            {
                var sesion = ctx.Sesiones.Find(idSesion);
                if (sesion != null && sesion.EstadoSesion == "ACTIVA")
                {
                    sesion.FechaHoraUltimaAct = _fechas.ObtenerFechaActual();
                    ctx.SaveChanges();
                }
            }
        }

        public void RegistrarAccesoBitacora(int idUsuario, string accion, string detalle, string ip, string dispositivo)
        {
            using (var ctx = new ColibriDbContext())
            {
                RegistrarBitacoraAcceso(ctx, idUsuario, accion, detalle, ip, dispositivo);
                ctx.SaveChanges();
            }
        }

        // Bloquear/desbloquear usuario
        public void BloquearUsuario(int idUsuario, bool bloqueado, int idUsuarioAdmin)
        {
            using (var ctx = new ColibriDbContext())
            {
                var user = ctx.Usuarios.Find(idUsuario);
                if (user == null)
                    throw new KeyNotFoundException("Usuario no encontrado.");

                user.Bloqueado = bloqueado;
                ctx.SaveChanges();

                RegistrarAuditoria(ctx, bloqueado ? "ACTIVACION" : "DESACTIVACION", "Usuarios",
                    idUsuario,
                    bloqueado ? "Bloqueado: false" : "Bloqueado: true",
                    bloqueado ? "Bloqueado: true" : "Bloqueado: false",
                    $"Usuario {(bloqueado ? "bloqueado" : "desbloqueado")}: {user.NombreUsuario}",
                    idUsuarioAdmin, null, null);
            }
        }
    }
}
