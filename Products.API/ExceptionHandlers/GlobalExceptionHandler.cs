using Microsoft.AspNetCore.Diagnostics;
using ECommerce_G24.Products.API.Exceptions;
namespace ECommerce_G24.Products.API.ExceptionHandlers
{
    public class GlobalExceptionHandler:IExceptionHandler
    {
        async public ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken
            )
        {
            if (exception is not NotFoundException ex)
            {
                return false;
            }
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Internal service error",
                status = 500,
                detail = "Ocurrió un error inesperado",
                instance = context.Request.Path.Value,
                errorCode = "PRD-005",
                errorMessage = "Error interno al procesar el producto.",
                correlationId = context.TraceIdentifier

            }, cancellationToken);
            return true;
        }
        
    }
}
