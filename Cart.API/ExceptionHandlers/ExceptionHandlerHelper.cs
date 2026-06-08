namespace Cart.API.Cart.API.ExceptionHandlers
{
    // Helper para obtener el Correlation ID actual.
    public class ExceptionHandlerHelper
    {
        public const string CorrelationIdHeaderName = "X-Correlation-Id";

        public static string GetCorrelationId(HttpContext context)
        {
            // Primero intenta obtener el valor guardado por el middleware.
            if (context.Items.TryGetValue(CorrelationIdHeaderName, out var correlationId))
                return correlationId?.ToString() ?? string.Empty;

            // Luego intenta obtenerlo del header HTTP.
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue))
                return headerValue.ToString();

            // Último fallback.
            return context.TraceIdentifier;
        }
    }
}
