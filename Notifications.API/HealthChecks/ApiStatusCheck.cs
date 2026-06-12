using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Notifications.API.HealthChecks;

/// <summary>
/// Comprueba que Notifications.API esté operativa.
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
            ["service"] = "Notifications.API",

            ["startedAt"] = StartedAt,

            ["uptimeSeconds"] =
                (DateTime.UtcNow - StartedAt).TotalSeconds,

            ["dotnetVersion"] =
                Environment.Version.ToString()
        };

        return Task.FromResult(
            HealthCheckResult.Healthy(
                "Notifications.API está operativa.",
                data));
    }
}