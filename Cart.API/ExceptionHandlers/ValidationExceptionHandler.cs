using ECommerce_G24.Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ECommerce_G24.Cart.API.ExceptionHandlers
{
    // Handler para errores de validación 400.
    public class ValidationExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not ValidationException ex)
                return false;

            var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = "La solicitud contiene datos inválidos.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId
            }, cancellationToken);

            return true;
        }
    }
}
