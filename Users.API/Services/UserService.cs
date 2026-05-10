using Users.API.Dtos;
using Users.API.Exceptions;
using Users.API.Models;
using Users.API.Utilities;

namespace Users.API.Services;

public class UserService : IUserService
{
    private static readonly List<User> Users = [];
    private const int MaxFailedAttempts = 3;

    public Task<UserResponseDto> RegisterAsync(RegisterUserRequestDto request)
    {
        ValidateRegisterRequest(request);

        var existingUser = Users.FirstOrDefault(u => u.Email == request.Email);
        if (existingUser != null)
            throw new DuplicateEmailException();

        var user = new User
        {
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            PasswordHash = PasswordHelper.HashPassword(request.Password),
            Activo = true,
            IntentosFallidos = 0,
            FechaRegistro = DateTime.UtcNow,
            FechaUltimoLogin = null
        };

        Users.Add(user);
        return Task.FromResult(MapToResponseDto(user));
    }

    public Task<UserResponseDto> LoginAsync(LoginUserRequestDto request)
    {
        ValidateLoginRequest(request);

        var user = Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
            throw new InvalidCredentialsException();

        if (!user.Activo)
            throw new UserBlockedException("Usuario bloqueado por razones de seguridad.");

        if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.IntentosFallidos++;

            if (user.IntentosFallidos >= MaxFailedAttempts)
            {
                user.Activo = false;
                throw new UserBlockedException("Usuario bloqueado por demasiados intentos fallidos.");
            }

            throw new InvalidCredentialsException();
        }

        // Reset failed attempts and update last login
        user.IntentosFallidos = 0;
        user.FechaUltimoLogin = DateTime.UtcNow;

        return Task.FromResult(MapToResponseDto(user));
    }

    private static void ValidateRegisterRequest(RegisterUserRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ValidationException("El nombre es requerido.");

        if (string.IsNullOrWhiteSpace(request.Apellido))
            throw new ValidationException("El apellido es requerido.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("El email es requerido.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("La contraseña es requerida.");

        if (!IsValidEmail(request.Email))
            throw new ValidationException("El formato del email es inválido.");

        if (request.Password.Length < 6)
            throw new ValidationException("La contraseña debe tener al menos 6 caracteres.");
    }

    private static void ValidateLoginRequest(LoginUserRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("El email es requerido.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new ValidationException("La contraseña es requerida.");

        if (!IsValidEmail(request.Email))
            throw new ValidationException("El formato del email es inválido.");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static UserResponseDto MapToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email,
            Activo = user.Activo,
            FechaRegistro = user.FechaRegistro,
            FechaUltimoLogin = user.FechaUltimoLogin
        };
    }
}
