using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerce_G24.Products.API.HealthChecks
{
    // Health Check simple para verificar que la API está operativa.
    
    public class ApiStatusCheck : IHealthCheck
    {
        private static readonly DateTime StartedAt = DateTime.UtcNow;

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            // Devuelve información básica de estado de la API.
            var data = new Dictionary<string, object>
            {
                ["service"] = "Products.API",
                ["startedAt"] = StartedAt,
                ["uptimeSeconds"] = (DateTime.UtcNow - StartedAt).TotalSeconds,
                ["dotnetVersion"] = Environment.Version.ToString()
            };

            return Task.FromResult(
                HealthCheckResult.Healthy("Products.API está operativa.", data));
        }
    }
}

