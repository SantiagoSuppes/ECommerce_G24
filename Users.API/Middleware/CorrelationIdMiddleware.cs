using Serilog.Context;

namespace Users.API.Middleware
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
            var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var value)
                ? value.ToString()
                : Guid.NewGuid().ToString();

            context.Response.Headers[HeaderName] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            using (LogContext.PushProperty("Endpoint", context.Request.Path.ToString()))
            {
                await _next(context);
            }
        }
    }
}