using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;

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
            // Si la excepción no es ValidationException, este handler no la procesa.
            if (exception is not ValidationException ex)
                return false;

            var correlationId = ExceptionHandlerHelper.GetCorrelationId(context);
            using (LogContext.PushProperty("errorCode", ex.ErrorCode))
            {
                _logger.LogWarning(exception, "Datos inválidos. ErrorCode: {ErrorCode}", ex.ErrorCode);
            }

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Bad Request",
                Status = StatusCodes.Status400BadRequest,
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
