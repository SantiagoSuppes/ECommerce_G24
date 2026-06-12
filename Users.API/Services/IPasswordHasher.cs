namespace Users.API.Services;

/// <summary>
/// permite generar y verificar hashes de contraseña.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);

    bool VerifyPassword(string password, string storedHash);
}