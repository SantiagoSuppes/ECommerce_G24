using System.Security.Cryptography;
using System.Text;

namespace Users.API.Services;

/// <summary>
/// Implementación de hashing mediante PBKDF2.
/// La contraseña nunca se guarda en texto plano.
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    // Cantidad de iteraciones utilizadas por PBKDF2.
    private const int Iterations = 100_000;

    // Tamaño de la sal aleatoria.
    private const int SaltSize = 16;

    // Tamaño final del hash.
    private const int HashSize = 32;

    public string HashPassword(string password)
    {
        // Genera una sal aleatoria diferente para cada usuario.
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Convierte la contraseña a bytes.
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        // Genera el hash usando PBKDF2 y SHA-256.
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            passwordBytes,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        // Se almacenan iteraciones, sal y hash en un único string.
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            // El formato almacenado es:
            // iteraciones.salt.hash
            var parts = storedHash.Split('.');

            if (parts.Length != 3)
                return false;

            if (!int.TryParse(parts[0], out var iterations))
                return false;

            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);
            var passwordBytes = Encoding.UTF8.GetBytes(password);

            // Genera nuevamente el hash usando la sal guardada.
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                passwordBytes,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length);

            // Compara los hashes en tiempo constante.
            return CryptographicOperations.FixedTimeEquals(
                actualHash,
                expectedHash);
        }
        catch
        {
            // Si el formato almacenado es inválido, la contraseña no coincide.
            return false;
        }
    }
}