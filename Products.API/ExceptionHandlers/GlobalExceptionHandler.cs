using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
namespace ECommerce_G24.Products.API.ExceptionHandlers
{
    // Handler global para errores inesperados.
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault();

            _logger.LogError(
                exception,
                "Error inesperado PRD-005 en {Endpoint}. CorrelationId: {CorrelationId}",
                context.Request.Path,
                correlationId);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = 500,
                Detail = _environment.IsDevelopment()
                    ? exception.Message
                    : "Ocurrió un error interno en el servidor.",
                Instance = context.Request.Path,
                ErrorCode = "PRD-005",
                ErrorMessage = "Error interno al procesar el producto.",
                CorrelationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}
