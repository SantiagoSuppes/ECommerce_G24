using ECommerce_G24.Cart.API.Dtos;
using ECommerce_G24.Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;

namespace ECommerce_G24.Cart.API.ExceptionHandlers
{
    // Handler genérico.
    // Captura errores inesperados no contemplados por los otros handlers.
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IWebHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var correlationId = ExceptionHandlerHelper.GetCorrelationId(context);

            using (LogContext.PushProperty("errorCode", CartErrorCodes.InternalCartError))
            {
                _logger.LogError(exception, "Error interno inesperado. ErrorCode: {ErrorCode}", CartErrorCodes.InternalCartError);
            }

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = _environment.IsDevelopment()
                    ? exception.Message
                    : "Ocurrió un error interno en el servidor.",
                Instance = context.Request.Path,
                ErrorCode = CartErrorCodes.InternalCartError,
                ErrorMessage = "Error interno al procesar el carrito.",
                CorrelationId = correlationId
            }, cancellationToken);

            return true;
        }
    }
}
