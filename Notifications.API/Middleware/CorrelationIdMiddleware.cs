using System.Diagnostics;

namespace ECommerce_G24.Notifications.API.Middleware;

/// Middleware para manejar y propagar Correlation IDs en el contexto de la solicitud.
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private const string CorrelationIdProperty = "CorrelationId";
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Intentar obtener el Correlation ID del header
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValue)
            ? headerValue.ToString()
            : Guid.NewGuid().ToString();

        // Establecer el Correlation ID en items para acceso durante el request
        context.Items[CorrelationIdProperty] = correlationId;

        // Agregar el header de respuesta
        context.Response.Headers.Append(CorrelationIdHeader, correlationId);

        var stopwatch = Stopwatch.StartNew();

        if (Activity.Current != null)
        {
            Activity.Current.AddTag("correlation_id", correlationId);
        }

        _logger.LogInformation("Request iniciado - Correlation ID: {CorrelationId}", correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Request finalizado - Correlation ID: {CorrelationId}, Status Code: {StatusCode}", 
                correlationId, context.Response.StatusCode);
        }
    }
}

/// Extensiones para registrar el CorrelationIdMiddleware.
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
