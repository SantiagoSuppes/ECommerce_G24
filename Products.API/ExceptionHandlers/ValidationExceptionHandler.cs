using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ECommerce_G24.Products.API.ExceptionHandlers
{
    // Handler global para errores 400 de validación.

    public class ValidationExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ValidationExceptionHandler> _logger;

        public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not ValidationException ex)
                return false;

            var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault();

            _logger.LogWarning(
                exception,
                "Error de validación {ErrorCode} en {Endpoint}. CorrelationId: {CorrelationId}",
                ex.ErrorCode,
                context.Request.Path,
                correlationId);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = 400,
                Detail = "La solicitud contiene datos inválidos.",
                Instance = context.Request.Path,
                ErrorCode = ex.ErrorCode,
                ErrorMessage = ex.Message,
                CorrelationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}
