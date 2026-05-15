using Microsoft.AspNetCore.Diagnostics;

namespace ECommerce_G24.Cart.API.ExceptionHandlers
{
    // Handler genérico.
    // Captura errores inesperados no contemplados por los otros handlers.
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            // Logueamos como Error porque es un error inesperado.
            _logger.LogError(exception,
                "Error inesperado en Cart.API. ErrorCode: {ErrorCode}. CorrelationId: {CorrelationId}",
                "CRT-005",
                correlationId);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "Ocurrió un error interno en el servidor.",
                instance = context.Request.Path.Value,
                errorCode = "CRT-005",
                errorMessage = "Error interno al procesar el carrito.",
                correlationId
            }, cancellationToken);

            return true;
        }
    }
}
