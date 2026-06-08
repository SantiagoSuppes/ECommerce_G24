using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;
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
            // Si la excepción no es BusinessRuleException, este handler no la procesa.
            if (exception is not BusinessRuleException ex)
                return false;

            var correlationId = ExceptionHandlerHelper.GetCorrelationId(context);

            using (LogContext.PushProperty("errorCode", ex.ErrorCode))
            {
                _logger.LogWarning(exception, "Regla de negocio incumplida. ErrorCode: {ErrorCode}", ex.ErrorCode);
            }

            context.Response.StatusCode = StatusCodes.Status409Conflict;

            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = ex.ErrorCode == ProductErrorCodes.ProductWithActiveOrders
                    ? "No se puede eliminar el recurso."
                    : "Ya existe un recurso con esos datos.",
                Instance = context.Request.Path,
                ErrorCode = ex.ErrorCode,
                ErrorMessage = ex.Message,
                CorrelationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}
