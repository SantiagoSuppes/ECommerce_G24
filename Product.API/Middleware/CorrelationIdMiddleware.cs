using Serilog.Context;
namespace ECommerce_G24.Products.API.Middleware
{
    // Middleware para generar o reutilizar el X-Correlation-Id del request.
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
            // Si el cliente mandó un X-Correlation-Id, se reutiliza.
            // Si no lo mandó, se genera uno nuevo.
            var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existingCorrelationId)
                ? existingCorrelationId.ToString()
                : Guid.NewGuid().ToString();

            // Se guarda en Items para que los ExceptionHandlers puedan leerlo.
            context.Items[HeaderName] = correlationId;

            // Se devuelve también en el header de respuesta.
            context.Response.Headers[HeaderName] = correlationId;

            // Se agrega al contexto de Serilog para que aparezca en los logs.
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
