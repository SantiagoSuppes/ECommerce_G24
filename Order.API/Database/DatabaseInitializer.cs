using Dapper;
using Microsoft.Data.Sqlite;

namespace Orders.API.Database;

public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(IConfiguration configuration, ILogger<DatabaseInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public void Initialize()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=orders.db";

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        connection.Execute("""
            CREATE TABLE IF NOT EXISTS orders (
                id TEXT NOT NULL PRIMARY KEY,
                usuario_id TEXT NOT NULL,
                total REAL NOT NULL,
                estado TEXT NOT NULL,
                fecha_creacion TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS order_items (
                id TEXT NOT NULL PRIMARY KEY,
                order_id TEXT NOT NULL,
                producto_id TEXT NOT NULL,
                cantidad INTEGER NOT NULL,
                precio_unitario REAL NOT NULL,
                FOREIGN KEY (order_id) REFERENCES orders(id)
            );
        """);

        _logger.LogInformation("Base de datos de Orders.API inicializada correctamente.");
    }
}