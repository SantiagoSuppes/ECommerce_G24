using Cart.API.Cart.API.Dtos;
using Cart.API.Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;

namespace Cart.API.Cart.API.ExceptionHandlers
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
            // Si la excepción no es NotFoundException,
            // este handler no la maneja.
            if (exception is not NotFoundException ex)
                return false;

            var correlationId = ExceptionHandlerHelper.GetCorrelationId(context);

            // Se agrega errorCode al contexto del log.
            using (LogContext.PushProperty("errorCode", ex.ErrorCode))
            {
                _logger.LogWarning(exception, "Recurso no encontrado. ErrorCode: {ErrorCode}", ex.ErrorCode);
            }

            context.Response.StatusCode = StatusCodes.Status404NotFound;

            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Not Found",
                Status = StatusCodes.Status404NotFound,
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
