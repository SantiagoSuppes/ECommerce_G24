using Serilog.Context;
namespace Cart.API.Cart.API.Middleware
{
    // Middleware para generar o reutilizar el X-Correlation-Id.
    public class CorrelationIdMiddleware
    {
        private const string HeaderName = "X-Correlation-Id";

        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Si el request ya trae un X-Correlation-Id, se reutiliza.
            // Si no lo trae, se genera uno nuevo.
            var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existingCorrelationId)
                ? existingCorrelationId.ToString()
                : Guid.NewGuid().ToString();

            // Se guarda para que lo puedan usar handlers, logs y clientes HTTP.
            context.Items[HeaderName] = correlationId;

            // También se devuelve en la respuesta.
            context.Response.Headers[HeaderName] = correlationId;

            // Se agrega como propiedad estructurada en Serilog.
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
