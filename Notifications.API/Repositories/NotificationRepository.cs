using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Notifications.API.Models;

namespace Notifications.API.Repositories;

/// <summary>
/// Implementación SQLite + Dapper
/// del repositorio de notificaciones.
/// </summary>
public class NotificationRepository : INotificationRepository
{
    private readonly IConfiguration _configuration;

    public NotificationRepository(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// conexión nueva a SQLite.
    /// </summary>
    private SqliteConnection CreateConnection()
    {
        var connectionString =
            _configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=notifications.db";

        return new SqliteConnection(connectionString);
    }

    public async Task<Notification> CreateAsync(
        Notification notification)
    {
        using var connection = CreateConnection();

        const string sql = """
            INSERT INTO notifications (
                id,
                usuario_id,
                mensaje,
                tipo,
                estado,
                fecha_envio
            )
            VALUES (
                @Id,
                @UsuarioId,
                @Mensaje,
                @Tipo,
                @Estado,
                @FechaEnvio
            );
        """;

        await connection.ExecuteAsync(sql, new
        {
            Id = notification.Id.ToString(),
            UsuarioId = notification.UsuarioId.ToString(),
            notification.Mensaje,
            notification.Tipo,
            notification.Estado,

            
            FechaEnvio = notification.FechaEnvio.ToString("O")
        });

        return notification;
    }

    public async Task<IReadOnlyCollection<Notification>> GetByUserIdAsync(
        Guid userId)
    {
        using var connection = CreateConnection();

        const string sql = """
            SELECT
                id AS Id,
                usuario_id AS UsuarioId,
                mensaje AS Mensaje,
                tipo AS Tipo,
                estado AS Estado,
                fecha_envio AS FechaEnvio
            FROM notifications
            WHERE usuario_id = @UsuarioId
            ORDER BY fecha_envio DESC;
        """;

        // Se lee primero usando strings para evitar
        // problemas de conversión de Guid y DateTime en SQLite.
        var records =
            await connection.QueryAsync<NotificationRecord>(
                sql,
                new
                {
                    UsuarioId = userId.ToString()
                });

        return records
            .Select(MapToNotification)
            .ToList();
    }

    /// <summary>
    /// Convierte una fila de SQLite al modelo de dominio.
    /// </summary>
    private static Notification MapToNotification(
        NotificationRecord record)
    {
        return new Notification
        {
            Id = Guid.Parse(record.Id),
            UsuarioId = Guid.Parse(record.UsuarioId),
            Mensaje = record.Mensaje,
            Tipo = record.Tipo,
            Estado = record.Estado,

            FechaEnvio = DateTime.Parse(
                record.FechaEnvio,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind)
        };
    }

    /// <summary>
    /// Modelo interno para leer los tipos nativos de SQLite.
    /// </summary>
    private sealed class NotificationRecord
    {
        public string Id { get; set; } = string.Empty;

        public string UsuarioId { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public string Tipo { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string FechaEnvio { get; set; } = string.Empty;
    }
}