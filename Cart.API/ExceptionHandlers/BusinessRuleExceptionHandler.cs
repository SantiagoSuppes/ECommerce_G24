using Cart.API.Cart.API.Dtos;
using Cart.API.Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;

namespace Cart.API.Cart.API.ExceptionHandlers
{
    // Handler para errores de negocio.
    // En Cart.API el caso principal es stock insuficiente.
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

            var correlationId = ExceptionHandlerHelper.GetCorrelationId(context);

            using (LogContext.PushProperty("errorCode", ex.ErrorCode))
            {
                _logger.LogWarning(exception, "Regla de negocio incumplida. ErrorCode: {ErrorCode}", ex.ErrorCode);
            }

            // Para CRT-003, el TP exige HTTP 422.
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                Title = "Unprocessable Entity",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = "No se puede procesar la solicitud.",
                Instance = context.Request.Path,
                ErrorCode = ex.ErrorCode,
                ErrorMessage = ex.Message,
                CorrelationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}
