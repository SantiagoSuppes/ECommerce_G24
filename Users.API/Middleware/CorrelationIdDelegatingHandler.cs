namespace Users.API.Middleware;

/// <summary>
/// Propaga X-Correlation-Id en llamadas HTTP salientes.
/// </summary>
public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdDelegatingHandler(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;

        if (context is not null &&
            context.Items.TryGetValue(
                CorrelationIdMiddleware.HeaderName,
                out var value))
        {
            var correlationId = value?.ToString();

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                request.Headers.Remove(
                    CorrelationIdMiddleware.HeaderName);

                request.Headers.Add(
                    CorrelationIdMiddleware.HeaderName,
                    correlationId);
            }
        }

        return await base.SendAsync(
            request,
            cancellationToken);
    }
}