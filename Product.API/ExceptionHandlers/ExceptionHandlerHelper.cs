namespace ECommerce_G24.Products.API.ExceptionHandlers
{
    // Helper interno para recuperar el Correlation ID desde el request actual.
    public class ExceptionHandlerHelper
    {
        public const string CorrelationIdHeaderName = "X-Correlation-Id";

        public static string GetCorrelationId(HttpContext context)
        {
            // Si el middleware ya guardó el correlationId, se usa ese valor.
            if (context.Items.TryGetValue(CorrelationIdHeaderName, out var correlationId))
                return correlationId?.ToString() ?? string.Empty;

            // Si vino por header, se usa el header.
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValue))
                return headerValue.ToString();

            // Fallback para evitar devolver vacío.
            return context.TraceIdentifier;
        }
    }
}
