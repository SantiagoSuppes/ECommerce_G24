using Dapper;
using Microsoft.Data.Sqlite;

namespace Orders.API.Database;

/// <summary>
/// Inicializa la base de datos SQLite utilizada por Orders.API.
/// </summary>
public class DatabaseInitializer
{
    private readonly IConfiguration _configuration;

    public DatabaseInitializer(
        IConfiguration configuration)
    {
        _configuration = configuration;
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

        // Crea las tablas si todavía no existen.
        connection.Execute("""
            CREATE TABLE IF NOT EXISTS orders
            (
                id                   TEXT PRIMARY KEY,
                usuario_id           TEXT NOT NULL,
                total                REAL NOT NULL,
                estado               TEXT NOT NULL,
                fecha_creacion       TEXT NOT NULL,
                fecha_actualizacion  TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS order_items
            (
                order_id         TEXT NOT NULL,
                producto_id      TEXT NOT NULL,
                cantidad         INTEGER NOT NULL,
                precio_unitario  REAL NOT NULL,

                PRIMARY KEY (
                    order_id,
                    producto_id
                ),

                FOREIGN KEY (order_id)
                    REFERENCES orders(id)
                    ON DELETE CASCADE
            );
            """);

        AddMissingColumns(connection);
    }

    /// <summary>
    /// Agrega columnas faltantes cuando la base fue creada
    /// con una versión anterior del proyecto.
    /// </summary>
    private static void AddMissingColumns(
        SqliteConnection connection)
    {
        var columns = connection
            .Query<TableColumnInfo>(
                "PRAGMA table_info(orders);")
            .Select(column => column.Name)
            .ToHashSet(
                StringComparer.OrdinalIgnoreCase);

        // CREATE TABLE IF NOT EXISTS no modifica tablas existentes.
        // Por eso debemos agregar manualmente la columna si falta.
        if (!columns.Contains("fecha_actualizacion"))
        {
            connection.Execute("""
                ALTER TABLE orders
                ADD COLUMN fecha_actualizacion TEXT NULL;
                """);
        }
    }

    private sealed class TableColumnInfo
    {
        public string Name { get; set; } =
            string.Empty;
    }
}