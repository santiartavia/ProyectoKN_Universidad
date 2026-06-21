using Abstracciones.Models;
using System;
using System.Collections.Generic;

namespace Abstracciones.Interfaces
{
    public interface IEmpleadoService
    {
        Empleado Registrar(string cedula, string nombre, string apellidos, string telefono, string correo,
                           string rolNombre, decimal salarioHora, DateTime fechaIngreso, int idUsuarioAdmin);
        Empleado ObtenerPorId(int idEmpleado);
        Empleado ObtenerPorCedula(string cedula);
        List<Empleado> ListarActivos();
        List<Empleado> ListarTodos();
        Empleado Actualizar(int idEmpleado, string telefono, string correo, string rolNombre, decimal salarioHora, int idUsuarioAdmin);
        Empleado Inactivar(int idEmpleado, string motivo, int idUsuarioAdmin);
        Empleado Reactivar(int idEmpleado, int idUsuarioAdmin);
        List<Empleado> ListarInactivos();
        List<Empleado> Buscar(string termino);
        Empleado ObtenerPorUsuarioId(int idUsuario);
    }
}