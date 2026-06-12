using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Dapper;

namespace Cart.API.Cart.API.HealthChecks
{
    // Health Check que valida la conexión a SQLite.
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
                var connectionString = _configuration.GetConnectionString("DefaultConnection")
                    ?? "Data Source=cart.db";

                using var connection = new SqliteConnection(connectionString);

                await connection.OpenAsync(cancellationToken);

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
