using Serilog.Context;

namespace Notifications.API.Middleware;

/// <summary>
/// Genera o reutiliza X-Correlation-Id
/// para cada request entrante.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context)
    {
        var correlationId =
            context.Request.Headers.TryGetValue(
                HeaderName,
                out var existingValue)
                ? existingValue.ToString()
                : Guid.NewGuid().ToString();

        // Disponible para handlers y clientes HTTP.
        context.Items[HeaderName] = correlationId;

        // Se devuelve también como header.
        context.Response.Headers[HeaderName] =
            correlationId;

        // Se agrega al contexto de Serilog.
        using (LogContext.PushProperty(
                   "CorrelationId",
                   correlationId))
        {
            await _next(context);
        }
    }
}