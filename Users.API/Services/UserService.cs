using Serilog;
using Users.API.Dtos;
using Users.API.Exceptions;
using Users.API.Models;
using Users.API.Utilities;

namespace Users.API.Services;

public class UserService : IUserService
{
    private static readonly List<User> Users = [];
    private const int MaxFailedAttempts = 3;
    private readonly ILogger<UserService> _logger;

    public UserService(ILogger<UserService> logger)
    {
        _logger = logger;
    }

    public Task<UserResponseDto> RegisterAsync(RegisterUserRequestDto request)
    {
        ValidateRegisterRequest(request);

        var existingUser = Users.FirstOrDefault(u => u.Email == request.Email);
        if (existingUser != null)
        {
            _logger.LogWarning("Intento de registro con email duplicado: {Email}", request.Email);
            throw new DuplicateEmailException();
        }

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
        _logger.LogInformation("Usuario registrado: {Email}", user.Email);
        return Task.FromResult(MapToResponseDto(user));
    }

    public Task<UserResponseDto> LoginAsync(LoginUserRequestDto request)
    {
        ValidateLoginRequest(request);

        var user = Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            _logger.LogWarning("Intento de login con email inexistente: {Email}", request.Email);
            throw new InvalidCredentialsException();
        }

        if (!user.Activo)
        {
            _logger.LogWarning("Intento de login con usuario bloqueado: {Email}", request.Email);
            throw new UserBlockedException("Usuario bloqueado por razones de seguridad.");
        }

        if (!PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.IntentosFallidos++;

            if (user.IntentosFallidos >= MaxFailedAttempts)
            {
                user.Activo = false;
                _logger.LogWarning("Usuario bloqueado por demasiados intentos fallidos: {Email}", request.Email);
                throw new UserBlockedException("Usuario bloqueado por demasiados intentos fallidos.");
            }

            _logger.LogWarning("Credenciales inválidas para: {Email}. Intentos fallidos: {Intentos}", request.Email, user.IntentosFallidos);
            throw new InvalidCredentialsException();
        }

        user.IntentosFallidos = 0;
        user.FechaUltimoLogin = DateTime.UtcNow;

        _logger.LogInformation("Login exitoso: {Email}", user.Email);
        return Task.FromResult(MapToResponseDto(user));
    }

    private void ValidateRegisterRequest(RegisterUserRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            _logger.LogWarning("Validacion fallida en registro: nombre vacio");
            throw new ValidationException("El nombre es requerido.");
        }

        if (string.IsNullOrWhiteSpace(request.Apellido))
        {
            _logger.LogWarning("Validacion fallida en registro: apellido vacio");
            throw new ValidationException("El apellido es requerido.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("Validacion fallida en registro: email vacio");
            throw new ValidationException("El email es requerido.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Validacion fallida en registro: password vacio");
            throw new ValidationException("La contraseña es requerida.");
        }

        if (!IsValidEmail(request.Email))
        {
            _logger.LogWarning("Validacion fallida en registro: email invalido {Email}", request.Email);
            throw new ValidationException("El formato del email es inválido.");
        }

        if (request.Password.Length < 6)
        {
            _logger.LogWarning("Validacion fallida en registro: password muy corto");
            throw new ValidationException("La contraseña debe tener al menos 6 caracteres.");
        }
    }

    private void ValidateLoginRequest(LoginUserRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            _logger.LogWarning("Validacion fallida en login: email vacio");
            throw new ValidationException("El email es requerido.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            _logger.LogWarning("Validacion fallida en login: password vacio");
            throw new ValidationException("La contraseña es requerida.");
        }

        if (!IsValidEmail(request.Email))
        {
            _logger.LogWarning("Validacion fallida en login: email invalido {Email}", request.Email);
            throw new ValidationException("El formato del email es inválido.");
        }
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