using Dapper;
using Microsoft.Data.Sqlite;

namespace Users.API.Database;

/// <summary>
/// Inicializa la base SQLite de Users.API.
/// </summary>
public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IConfiguration configuration,
        ILogger<DatabaseInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Initialize()
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=users.db";

        using var connection = new SqliteConnection(connectionString);

        connection.Open();

        connection.Execute("""
            CREATE TABLE IF NOT EXISTS users (
                id TEXT PRIMARY KEY,
                nombre TEXT NOT NULL,
                apellido TEXT NOT NULL,
                email TEXT NOT NULL COLLATE NOCASE UNIQUE,
                password_hash TEXT NOT NULL,
                fecha_registro TEXT NOT NULL,
                activo INTEGER NOT NULL DEFAULT 1
                    CHECK (activo IN (0, 1)),
                intentos_fallidos INTEGER NOT NULL DEFAULT 0
                    CHECK (intentos_fallidos >= 0)
            );
        """);

        _logger.LogInformation(
            "Base de datos de Users.API inicializada correctamente.");
    }
}