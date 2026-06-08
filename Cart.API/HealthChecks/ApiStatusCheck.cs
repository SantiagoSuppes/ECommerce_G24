using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerce_G24.Cart.API.HealthChecks
{
   // Health Check básico para verificar que Cart.API está viva.
   
    public class ApiStatusCheck : IHealthCheck
    {
        private static readonly DateTime StartedAt = DateTime.UtcNow;

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>
            {
                ["service"] = "Cart.API",
                ["startedAt"] = StartedAt,
                ["uptimeSeconds"] = (DateTime.UtcNow - StartedAt).TotalSeconds,
                ["dotnetVersion"] = Environment.Version.ToString()
            };

            return Task.FromResult(
                HealthCheckResult.Healthy("Cart.API está operativa.", data));
        }
    }
}
