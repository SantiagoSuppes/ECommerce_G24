using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;
using Users.API.Repositories;

namespace Users.API.Services;

/// <summary>
/// Servicio principal de Users.API.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository repository,
        IPasswordHasher passwordHasher,
        ILogger<UserService> logger)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<RegisterUserResponseDto> RegisterAsync(
        RegisterUserRequestDto request)
    {
        // Normaliza el email para evitar duplicados por mayúsculas.
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var existingUser =
            await _repository.GetByEmailAsync(normalizedEmail);

        // Si ya existe, corresponde USR-001.
        if (existingUser is not null)
        {
            throw new BusinessRuleException(
                UserErrorCodes.DuplicateEmail,
                $"El email '{normalizedEmail}' ya está registrado.",
                StatusCodes.Status409Conflict);
        }

        // La contraseña se transforma en hash antes de persistirla.
        var passwordHash =
            _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre.Trim(),
            Apellido = request.Apellido.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            FechaRegistro = DateTime.UtcNow,
            Activo = true,
            IntentosFallidos = 0
        };

        var createdUser =
            await _repository.CreateAsync(user);

        _logger.LogInformation(
            "Usuario registrado correctamente. UsuarioId: {UsuarioId}",
            createdUser.Id);

        return new RegisterUserResponseDto
        {
            Id = createdUser.Id,
            Nombre = createdUser.Nombre,
            Apellido = createdUser.Apellido,
            Email = createdUser.Email,
            FechaRegistro = createdUser.FechaRegistro,
            Activo = createdUser.Activo
        };
    }

    public async Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request)
    {
        var normalizedEmail =
            request.Email.Trim().ToLowerInvariant();

        var user =
            await _repository.GetByEmailAsync(normalizedEmail);

        // No se informa si el email existe o no.
        // Ambos casos devuelven credenciales incorrectas.
        if (user is null)
        {
            throw new BusinessRuleException(
                UserErrorCodes.InvalidCredentials,
                "Credenciales incorrectas.",
                StatusCodes.Status401Unauthorized);
        }

        // Si está inactivo y tiene 3 o más intentos,
        // se considera bloqueado por intentos fallidos.
        if (!user.Activo && user.IntentosFallidos >= 3)
        {
            throw new BusinessRuleException(
                UserErrorCodes.BlockedByFailedAttempts,
                "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.",
                StatusCodes.Status403Forbidden);
        }

        // El UML no incluye un campo MotivoBloqueo.
        // Por eso, un usuario inactivo con menos de 3 intentos
        // se interpreta como bloqueo manual por fraude.
        if (!user.Activo)
        {
            throw new BusinessRuleException(
                UserErrorCodes.BlockedByFraud,
                "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.",
                StatusCodes.Status403Forbidden);
        }

        var passwordIsValid =
            _passwordHasher.VerifyPassword(
                request.Password,
                user.PasswordHash);

        if (!passwordIsValid)
        {
            // Incrementa el contador de intentos.
            var updatedUser =
                await _repository.RegisterFailedAttemptAsync(user.Id);

            // En el tercer intento fallido la cuenta pasa a Activo = false.
            if (!updatedUser.Activo ||
                updatedUser.IntentosFallidos >= 3)
            {
                _logger.LogWarning(
                    "Usuario bloqueado por intentos fallidos. UsuarioId: {UsuarioId}",
                    updatedUser.Id);

                throw new BusinessRuleException(
                    UserErrorCodes.BlockedByFailedAttempts,
                    "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.",
                    StatusCodes.Status403Forbidden);
            }

            throw new BusinessRuleException(
                UserErrorCodes.InvalidCredentials,
                "Credenciales incorrectas.",
                StatusCodes.Status401Unauthorized);
        }

        // Un login exitoso corta la secuencia de intentos fallidos.
        if (user.IntentosFallidos > 0)
        {
            await _repository.ResetFailedAttemptsAsync(user.Id);
        }

        _logger.LogInformation(
            "Login correcto. UsuarioId: {UsuarioId}",
            user.Id);

        return new LoginResponseDto
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email
        };
    }
}