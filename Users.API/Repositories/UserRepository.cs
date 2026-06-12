using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Users.API.Models;

namespace Users.API.Repositories;

/// <summary>
/// Implementación SQLite + Dapper del repositorio de usuarios.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IConfiguration _configuration;

    public UserRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqliteConnection CreateConnection()
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=users.db";

        return new SqliteConnection(connectionString);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = CreateConnection();

        const string sql = """
            SELECT
                id AS Id,
                nombre AS Nombre,
                apellido AS Apellido,
                email AS Email,
                password_hash AS PasswordHash,
                fecha_registro AS FechaRegistro,
                activo AS Activo,
                intentos_fallidos AS IntentosFallidos
            FROM users
            WHERE email = @Email COLLATE NOCASE;
        """;

        var record = await connection.QuerySingleOrDefaultAsync<UserRecord>(
            sql,
            new
            {
                Email = email
            });

        return record is null ? null : MapToUser(record);
    }

    public async Task<User> CreateAsync(User user)
    {
        using var connection = CreateConnection();

        const string sql = """
            INSERT INTO users (
                id,
                nombre,
                apellido,
                email,
                password_hash,
                fecha_registro,
                activo,
                intentos_fallidos
            )
            VALUES (
                @Id,
                @Nombre,
                @Apellido,
                @Email,
                @PasswordHash,
                @FechaRegistro,
                @Activo,
                @IntentosFallidos
            );
        """;

        await connection.ExecuteAsync(sql, new
        {
            Id = user.Id.ToString(),
            user.Nombre,
            user.Apellido,
            user.Email,
            user.PasswordHash,

            // Se guarda en formato ISO 8601.
            FechaRegistro = user.FechaRegistro.ToString("O"),

            Activo = user.Activo ? 1 : 0,
            user.IntentosFallidos
        });

        return user;
    }

    public async Task<User> RegisterFailedAttemptAsync(Guid userId)
    {
        using var connection = CreateConnection();

        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();

        // Incrementa los intentos.
        // Cuando llega a tres, Activo pasa automáticamente a false.
        await connection.ExecuteAsync("""
            UPDATE users
            SET
                intentos_fallidos = intentos_fallidos + 1,
                activo = CASE
                    WHEN intentos_fallidos + 1 >= 3 THEN 0
                    ELSE activo
                END
            WHERE id = @Id;
        """, new
        {
            Id = userId.ToString()
        }, transaction);

        var record = await connection.QuerySingleAsync<UserRecord>("""
            SELECT
                id AS Id,
                nombre AS Nombre,
                apellido AS Apellido,
                email AS Email,
                password_hash AS PasswordHash,
                fecha_registro AS FechaRegistro,
                activo AS Activo,
                intentos_fallidos AS IntentosFallidos
            FROM users
            WHERE id = @Id;
        """, new
        {
            Id = userId.ToString()
        }, transaction);

        transaction.Commit();

        return MapToUser(record);
    }

    public async Task ResetFailedAttemptsAsync(Guid userId)
    {
        using var connection = CreateConnection();

        await connection.ExecuteAsync("""
            UPDATE users
            SET intentos_fallidos = 0
            WHERE id = @Id;
        """, new
        {
            Id = userId.ToString()
        });
    }
    /// <summary>
    /// Comprueba si existe un usuario con el ID recibido.
    /// </summary>
    public async Task<bool> ExistsByIdAsync(Guid userId)
    {
        using var connection = CreateConnection();

        const string sql = """
        SELECT COUNT(1)
        FROM users
        WHERE id = @Id;
    """;

        var count = await connection.ExecuteScalarAsync<int>(
            sql,
            new
            {
                
                Id = userId.ToString()
            });

        return count > 0;
    }

    private static User MapToUser(UserRecord record)
    {
        return new User
        {
            Id = Guid.Parse(record.Id),
            Nombre = record.Nombre,
            Apellido = record.Apellido,
            Email = record.Email,
            PasswordHash = record.PasswordHash,

            FechaRegistro = DateTime.Parse(
                record.FechaRegistro,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind),

            Activo = record.Activo == 1,
            IntentosFallidos = record.IntentosFallidos
        };
    }

    /// <summary>
    /// Clase interna usada para leer los tipos nativos de SQLite.
    /// </summary>
    private sealed class UserRecord
    {
        public string Id { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string FechaRegistro { get; set; } = string.Empty;

        public int Activo { get; set; }

        public int IntentosFallidos { get; set; }
    }
}