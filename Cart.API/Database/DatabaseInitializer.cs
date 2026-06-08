using Dapper;
using Microsoft.Data.Sqlite;
namespace ECommerce_G24.Cart.API.Database

{
    // Inicializa la base SQLite de Cart.API.
    // Crea las tablas necesarias si no existen.
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
            // Obtiene la cadena de conexión desde appsettings.json.
            var connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=cart.db";

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // Tabla de items del carrito.
            // Se usa clave compuesta usuario + producto para evitar duplicados.
            connection.Execute("""
            CREATE TABLE IF NOT EXISTS cart_items (
                usuario_id TEXT NOT NULL,
                producto_id TEXT NOT NULL,
                cantidad INTEGER NOT NULL,
                fecha_actualizacion TEXT NOT NULL,
                PRIMARY KEY (usuario_id, producto_id)
            );
        """);

            _logger.LogInformation("Base de datos de Cart.API inicializada correctamente.");
        }
    }
}
