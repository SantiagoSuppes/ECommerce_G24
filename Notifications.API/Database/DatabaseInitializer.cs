using Dapper;
using Microsoft.Data.Sqlite;

namespace Notifications.API.Database;

/// <summary>
/// Inicializa la base de datos de Notifications.API.
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
            ?? "Data Source=notifications.db";

        using var connection =
            new SqliteConnection(connectionString);

        connection.Open();

        // Crea la tabla principal de notificaciones.
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS notifications (
                id TEXT PRIMARY KEY,
                usuario_id TEXT NOT NULL,
                mensaje TEXT NOT NULL
                    CHECK (length(mensaje) <= 500),
                tipo TEXT NOT NULL
                    CHECK (tipo IN ('Email', 'Push', 'SMS')),
                estado TEXT NOT NULL
                    CHECK (estado IN ('Pendiente', 'Enviada', 'Fallida')),
                fecha_envio TEXT NOT NULL
            );
        """);

        // Indice para mejorar las búsquedas de notificaciones por usuario.
        connection.Execute("""
            CREATE INDEX IF NOT EXISTS
                ix_notifications_usuario_id
            ON notifications (usuario_id);
        """);

        _logger.LogInformation(
            "Base de datos de Notifications.API inicializada correctamente.");
    }
}