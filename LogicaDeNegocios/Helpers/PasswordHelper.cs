using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace LogicaDeNegocios.Helpers
{
    public static class PasswordHelper
    {
        private const int SALT_SIZE = 16;
        private const int HASH_SIZE = 32;
        private const int ITERATIONS = 10000;
        private const int MIN_LENGTH = 12;

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SALT_SIZE];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS, HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(HASH_SIZE);
            }

            byte[] hashBytes = new byte[SALT_SIZE + HASH_SIZE];
            Array.Copy(salt, 0, hashBytes, 0, SALT_SIZE);
            Array.Copy(hash, 0, hashBytes, SALT_SIZE, HASH_SIZE);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);
            byte[] salt = new byte[SALT_SIZE];
            Array.Copy(hashBytes, 0, salt, 0, SALT_SIZE);

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, ITERATIONS, HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(HASH_SIZE);
            }

            for (int i = 0; i < HASH_SIZE; i++)
            {
                if (hashBytes[i + SALT_SIZE] != hash[i])
                    return false;
            }

            return true;
        }

        public static bool CumplePoliticaSeguridad(string password, out string mensaje)
        {
            mensaje = null;

            if (string.IsNullOrWhiteSpace(password))
            {
                mensaje = "La contraseña no puede estar vacía.";
                return false;
            }

            if (password.Length < MIN_LENGTH)
            {
                mensaje = $"La contraseña debe tener al menos {MIN_LENGTH} caracteres.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                mensaje = "La contraseña debe contener al menos una mayúscula.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                mensaje = "La contraseña debe contener al menos una minúscula.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                mensaje = "La contraseña debe contener al menos un número.";
                return false;
            }

            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            {
                mensaje = "La contraseña debe contener al menos un caracter especial.";
                return false;
            }

            return true;
        }

        public static bool EsCaracterValido(char c)
        {
            return char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) ||
                   c == '.' || c == '_' || c == '-' || c == '@' ||
                   c == 'ñ' || c == 'ñ' || c == 'ñ' || c == 'ñ' || c == 'ñ' ||
                   c == 'ñ' || c == 'ñ' || c == 'ñ' || c == 'ñ' || c == 'ñ' ||
                   c == 'ñ' || c == 'ñ' || c == 'ñ' || c == 'ñ' || c == 'ñ' ||
                   c == 'ñ' || c == 'ñ' || c == 'ñ' || c == 'ñ' || c == 'ñ' ||
                   c == 'ñ' || c == 'ñ' || c == 'ñ' || c == 'ñ' || c == 'ñ' ||
                   c == '\'' || c == ' ';
        }

        public static bool ContieneCaracteresInvalidos(string input, out string mensaje)
        {
            mensaje = null;
            if (string.IsNullOrWhiteSpace(input)) return false;

            foreach (char c in input)
            {
                if (!EsCaracterValido(c))
                {
                    mensaje = $"El caracter '{c}' no es válido.";
                    return true;
                }
            }
            return false;
        }

        public static bool EsFormatoCorreoValido(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo)) return false;
            return Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public static bool EsDireccionValida(string direccion, out string mensaje)
        {
            mensaje = null;
            if (string.IsNullOrWhiteSpace(direccion))
            {
                mensaje = "La dirección no puede estar vacía.";
                return false;
            }

            string dir = direccion.ToLower().Trim();
            bool tieneProvincia = dir.Contains("provincia") || Regex.IsMatch(dir, @"\b(san José|alajuela|cartago|heredia|guanacaste|puntarenas|Limón)\b");
            bool tieneCanton = dir.Contains("cantón") || dir.Contains("canton") || Regex.IsMatch(dir, @"\b(central|occidental|oriental|norte|sur|este|oeste)\b", RegexOptions.IgnoreCase);

            if (!tieneProvincia || !tieneCanton)
            {
                mensaje = "La dirección debe incluir al menos provincia y cantón.";
                return false;
            }

            return true;
        }
    }
}
