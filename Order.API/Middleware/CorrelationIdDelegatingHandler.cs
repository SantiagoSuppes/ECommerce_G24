namespace Orders.API.Middleware;

/// <summary>
/// Propaga X-Correlation-Id hacia Users.API
/// y Products.API.
/// </summary>
public class CorrelationIdDelegatingHandler
    : DelegatingHandler
{
    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public CorrelationIdDelegatingHandler(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage>
        SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
    {
        var context =
            _httpContextAccessor.HttpContext;

        if (context is not null &&
            context.Items.TryGetValue(
                "X-Correlation-Id",
                out var value))
        {
            var correlationId =
                value?.ToString();

            if (!string.IsNullOrWhiteSpace(
                    correlationId))
            {
                request.Headers.Remove(
                    "X-Correlation-Id");

                request.Headers.Add(
                    "X-Correlation-Id",
                    correlationId);
            }
        }

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}