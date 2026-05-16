using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ECommerce_G24.Products.API.ExceptionHandlers
{
    // Handler global para errores 404.
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<NotFoundExceptionHandler> _logger;

        public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not NotFoundException ex)
                return false;

            var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault();

            _logger.LogWarning(
                exception,
                "Error controlado {ErrorCode} en {Endpoint}. CorrelationId: {CorrelationId}",
                ex.ErrorCode,
                context.Request.Path,
                correlationId);

            context.Response.StatusCode = StatusCodes.Status404NotFound;

            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Not Found",
                Status = 404,
                Detail = "El recurso solicitado no fue encontrado.",
                Instance = context.Request.Path,
                ErrorCode = ex.ErrorCode,
                ErrorMessage = ex.Message,
                CorrelationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}

    
