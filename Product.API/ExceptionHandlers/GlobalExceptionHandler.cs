using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;
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
            var correlationId = ExceptionHandlerHelper.GetCorrelationId(context);

            using (LogContext.PushProperty("errorCode", ProductErrorCodes.InternalProductError))
            {
                _logger.LogError(exception, "Error interno inesperado. ErrorCode: {ErrorCode}", ProductErrorCodes.InternalProductError);
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
                ErrorCode = ProductErrorCodes.InternalProductError,
                ErrorMessage = "Error interno al procesar el producto.",
                CorrelationId = correlationId
            }, cancellationToken);


            return true;
        }
    }
}
