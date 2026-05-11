using Microsoft.AspNetCore.Diagnostics;
using ECommerce_G24.Products.API.Exceptions;

namespace ECommerce_G24.Products.API.ExceptionHandlers
{
    public class ValidationExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync
            (
            HttpContext context,
            Exception excepction,
            CancellationToken cancellationToken
            )
        {
            if (excepction is not NotFoundException ex)
            {
                return false;
            }
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Not valid",
                status = 400,
                detail = "Los datos del producto son inválidos",
                instant = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                erroMessage = ex.Message,
                correlationId = context.TraceIdentifier
            }, cancellationToken);
            return true;
        }
    }
}
