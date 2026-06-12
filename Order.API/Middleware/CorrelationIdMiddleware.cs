using Serilog.Context;
namespace Orders.API.Middleware
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
            // Si el request ya viene con X-Correlation-Id,se usa ese sino generamos uno nuevo.
            var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var value)
                ? value.ToString()
                : Guid.NewGuid().ToString();

            // Establecer el Correlation ID en items para acceso durante el request
            context.Items[HeaderName] = correlationId;

            // Lo agregamos a la respuesta.
            context.Response.Headers[HeaderName] = correlationId;

            // Lo agregamos al contexto de Serilog para que aparezca en los logs.
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}