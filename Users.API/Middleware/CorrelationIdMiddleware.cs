using Serilog.Context;

namespace Users.API.Middleware;

/// <summary>
/// Genera o reutiliza un X-Correlation-Id por request.
/// No maneja excepciones.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId =
            context.Request.Headers.TryGetValue(
                HeaderName,
                out var existingValue)
                ? existingValue.ToString()
                : Guid.NewGuid().ToString();

        // Disponible para handlers y logs.
        context.Items[HeaderName] = correlationId;

        // Se devuelve en la respuesta.
        context.Response.Headers[HeaderName] = correlationId;

        // Se incorpora a Serilog.
        using (LogContext.PushProperty(
                   "CorrelationId",
                   correlationId))
        {
            await _next(context);
        }
    }
}