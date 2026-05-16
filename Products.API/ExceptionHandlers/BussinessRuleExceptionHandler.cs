using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
namespace ECommerce_G24.Products.API.ExceptionHandlers
{
    // Handler global para errores 409 por reglas de negocio.
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<BusinessRuleExceptionHandler> _logger;

        public BusinessRuleExceptionHandler(ILogger<BusinessRuleExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex)
                return false;

            var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault();

            _logger.LogWarning(
                exception,
                "Regla de negocio incumplida {ErrorCode} en {Endpoint}. CorrelationId: {CorrelationId}",
                ex.ErrorCode,
                context.Request.Path,
                correlationId);

            context.Response.StatusCode = StatusCodes.Status409Conflict;

            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                Title = "Conflict",
                Status = 409,
                Detail = "No se puede completar la operación solicitada.",
                Instance = context.Request.Path,
                ErrorCode = ex.ErrorCode,
                ErrorMessage = ex.Message,
                CorrelationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}
