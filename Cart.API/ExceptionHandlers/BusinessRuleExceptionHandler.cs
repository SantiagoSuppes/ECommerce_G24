using ECommerce_G24.Cart.Api.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace ECommerce_G24.Cart.API.ExceptionHandlers
{
    // Handler para errores de negocio.
    // En Cart.API el caso principal es stock insuficiente.
    public class BusinessRuleExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not BusinessRuleException ex)
                return false;

            var correlationId = context.Response.Headers["X-Correlation-Id"].FirstOrDefault()
                                ?? context.TraceIdentifier;

            // CRT-003 usa HTTP 422.
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                title = "Unprocessable Entity",
                status = 422,
                detail = "No se puede procesar la solicitud.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId
            }, cancellationToken);

            return true;
        }
    }
}
