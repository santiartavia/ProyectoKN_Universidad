using Abstracciones.Models;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface IUsuarioService
    {
        Usuario CrearUsuario(string nombreUsuario, string correo, string password, int idRol, int idUsuarioAdmin, string ip = null, string dispositivo = null);
        Usuario EditarUsuario(int idUsuario, string correo, string telefono, string direccion, int idUsuarioAdmin, string ip = null, string dispositivo = null);
        Usuario AsignarRol(int idUsuario, int idRolNuevo, int idUsuarioAdmin, string ip = null, string dispositivo = null);
        Usuario CambiarPassword(int idUsuario, string passwordActual, string passwordNuevo, string ip = null, string dispositivo = null);
        Usuario CambiarPasswordForzado(int idUsuario, string passwordNuevo, string metodoCambio, string ip = null, string dispositivo = null);
        bool VerificarPassword(int idUsuario, string password);
        Usuario EliminarUsuario(int idUsuario, int idUsuarioAdmin, string ip = null, string dispositivo = null);
        void DepurarUsuarios(List<int> idsUsuarios, int idUsuarioAdmin, string ip = null, string dispositivo = null);
        List<Usuario> ListarActivos();
        List<Usuario> ListarInactivos();
        Usuario ObtenerPorId(int idUsuario);
        Usuario ObtenerPorNombre(string nombreUsuario);
        bool NombreUsuarioDisponible(string nombreUsuario);
        bool CorreoDisponible(string correo);
    }
}
