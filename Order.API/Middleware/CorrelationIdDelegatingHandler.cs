namespace Orders.API.Middleware;

// Handler HTTP que propaga X-Correlation-Id hacia otros microservicios.
public class CorrelationIdDelegatingHandler : DelegatingHandler
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null)
        {
            var correlationId = httpContext.Items.TryGetValue(HeaderName, out var value)
                ? value?.ToString()
                : httpContext.Request.Headers[HeaderName].ToString();

            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                request.Headers.Remove(HeaderName);
                request.Headers.Add(HeaderName, correlationId);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}