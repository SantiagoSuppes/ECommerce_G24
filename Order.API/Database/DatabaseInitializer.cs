using Dapper;
using Microsoft.Data.Sqlite;

namespace Orders.API.Database;

/// <summary>
/// Inicializa la base SQLite de Orders.API.
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
            _configuration.GetConnectionString(
                "DefaultConnection")
            ?? "Data Source=orders.db";

        using var connection =
            new SqliteConnection(connectionString);

        connection.Open();

        // Activa el control de claves foráneas en SQLite.
        connection.Execute(
            "PRAGMA foreign_keys = ON;");

        // Cabecera de la orden.
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS orders (
                id TEXT PRIMARY KEY,
                usuario_id TEXT NOT NULL,
                total REAL NOT NULL
                    CHECK (total >= 0),
                estado TEXT NOT NULL
                    CHECK (
                        estado IN (
                            'Pendiente',
                            'Confirmada',
                            'Enviada',
                            'Entregada',
                            'Cancelada'
                        )
                    ),
                fecha_creacion TEXT NOT NULL,
                fecha_actualizacion TEXT NULL
            );
        """);

        // Detalle de la orden.
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS order_items (
                order_id TEXT NOT NULL,
                producto_id TEXT NOT NULL,
                cantidad INTEGER NOT NULL
                    CHECK (cantidad > 0),
                precio_unitario REAL NOT NULL
                    CHECK (precio_unitario > 0),

                PRIMARY KEY (
                    order_id,
                    producto_id
                ),

                FOREIGN KEY (order_id)
                    REFERENCES orders(id)
                    ON DELETE CASCADE
            );
        """);

        connection.Execute("""
            CREATE INDEX IF NOT EXISTS
                ix_orders_usuario_id
            ON orders (usuario_id);
        """);

        connection.Execute("""
            CREATE INDEX IF NOT EXISTS
                ix_order_items_producto_id
            ON order_items (producto_id);
        """);

        _logger.LogInformation(
            "Base de datos de Orders.API inicializada correctamente.");
    }
}