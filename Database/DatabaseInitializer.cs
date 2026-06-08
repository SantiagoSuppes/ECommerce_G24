using Dapper;
using Microsoft.Data.Sqlite;

namespace ECommerce_G24.Products.API.Database
{
    /// Inicializa la base SQLite de Products.API.
    /// Crea la tabla products si no existe.
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
            // Lee la cadena de conexión desde appsettings.json.
            var connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=products.db";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Crea la tabla principal de productos.
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS products (
                id TEXT PRIMARY KEY,
                nombre TEXT NOT NULL,
                descripcion TEXT NULL,
                precio REAL NOT NULL,
                stock INTEGER NOT NULL,
                categoria TEXT NOT NULL,
                fecha_creacion TEXT NOT NULL
            );
        """);

            // Índice único para evitar duplicados por nombre y categoría.
            connection.Execute("""
            CREATE UNIQUE INDEX IF NOT EXISTS ux_products_nombre_categoria
            ON products (nombre, categoria);
        """);

            _logger.LogInformation("Base de datos de Products.API inicializada correctamente.");
        }
    }
}
