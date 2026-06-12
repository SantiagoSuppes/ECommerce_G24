using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Orders.API.HealthChecks;

public class ApiStatusCheck : IHealthCheck
{
    private static readonly DateTime StartedAt = DateTime.UtcNow;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["service"] = "Orders.API",
            ["startedAt"] = StartedAt,
            ["uptimeSeconds"] = (DateTime.UtcNow - StartedAt).TotalSeconds,
            ["dotnetVersion"] = Environment.Version.ToString()
        };

        return Task.FromResult(
            HealthCheckResult.Healthy("Orders.API está operativa.", data));
    }
}