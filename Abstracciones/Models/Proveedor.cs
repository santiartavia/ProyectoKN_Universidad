namespace Abstracciones.Models
{
    public class Proveedor
    {
        public int IdProveedor { get; set; }
        public string CedulaJuridica { get; set; }
        public string NombreEmpresa { get; set; }
        public string ContactoNombre { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public bool Estado { get; set; }
    }
}
