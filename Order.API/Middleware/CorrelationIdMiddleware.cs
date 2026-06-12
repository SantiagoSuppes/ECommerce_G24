using Serilog.Context;

namespace Orders.API.Middleware;

/// <summary>
/// Genera o reutiliza X-Correlation-Id
/// para cada request.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName =
        "X-Correlation-Id";

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

        // Disponible para otros componentes.
        context.Items[HeaderName] =
            correlationId;

        // Se devuelve en la respuesta.
        context.Response.Headers[HeaderName] =
            correlationId;

        // Se agrega a todos los logs del request.
        using (LogContext.PushProperty(
                   "CorrelationId",
                   correlationId))
        {
            await _next(context);
        }
    }
}