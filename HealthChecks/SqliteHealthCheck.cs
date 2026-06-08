using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Dapper;

namespace ECommerce_G24.Products.API.HealthChecks
{
    // Health Check que valida la conexión contra SQLite.
 
    public class SqliteHealthCheck : IHealthCheck
    {
        private readonly IConfiguration _configuration;

        public SqliteHealthCheck(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Lee la cadena de conexión.
                var connectionString = _configuration.GetConnectionString("DefaultConnection")
                    ?? "Data Source=products.db";

                using var connection = new SqliteConnection(connectionString);

                await connection.OpenAsync(cancellationToken);

                // Ejecuta una consulta mínima para comprobar que la base responde.
                await connection.ExecuteScalarAsync<int>("SELECT 1;");

                return HealthCheckResult.Healthy("SQLite responde correctamente.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "No se pudo conectar a SQLite.",
                    ex);
            }
        }
    }
}
