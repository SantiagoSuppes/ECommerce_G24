using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Users.API.HealthChecks;

/// <summary>
/// Verifica que Users.API esté operativa.
/// </summary>
public class ApiStatusCheck : IHealthCheck
{
    private static readonly DateTime StartedAt =
        DateTime.UtcNow;

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>
        {
            ["service"] = "Users.API",
            ["startedAt"] = StartedAt,
            ["uptimeSeconds"] =
                (DateTime.UtcNow - StartedAt).TotalSeconds,
            ["dotnetVersion"] =
                Environment.Version.ToString()
        };

        return Task.FromResult(
            HealthCheckResult.Healthy(
                "Users.API está operativa.",
                data));
    }
}