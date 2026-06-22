namespace RestauranteVistas.Models.ViewModels
{
    public class LoginViewModel
    {
        public string Usuario { get; set; }
        public string Password { get; set; }
        public string Error { get; set; }
        public string Mensaje { get; set; }
        public bool MostrarOlvidoPassword { get; set; }
        public bool PasswordProximaVencer { get; set; }
        public int DiasRestantesPassword { get; set; }
    }
}
