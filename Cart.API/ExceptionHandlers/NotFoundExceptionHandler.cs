using ECommerce_G24.Cart.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ECommerce_G24.Cart.API.ExceptionHandlers
{
    // Handler global para errores 404.
    public class NotFoundExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Si la excepción no es NotFoundException,
            // este handler no la maneja.
            if (exception is not NotFoundException ex)
                return false;

            // Correlation ID para devolverlo también en el error.
            var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            context.Response.StatusCode = StatusCodes.Status404NotFound;

            // Respuesta con el formato exigido por el TP.
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = "El recurso solicitado no fue encontrado.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId
            }, cancellationToken);

            return true;
        }
    }
}
