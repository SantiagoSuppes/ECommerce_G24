using Microsoft.AspNetCore.Diagnostics;
using ECommerce_G24.Products.API.Exceptions;
namespace ECommerce_G24.Products.API.ExceptionHandlers
{
    public class BussinessRuleExceptionHandler : IExceptionHandler
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
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Conflict",
                status = 409,
                detail = GetDetail(ex.ErrorCode),
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationid = context.TraceIdentifier
            }, cancellationToken);
            return true;
        }
        private static string GetDetail(string errorCode)
        {
            return errorCode switch
            {
                "PRD-003" => "Ya existe un recurso con esos datos.",
                "PRD-004" => "No se puede eliminar el recurso.",
                _ => "No se pudo completar la operación por una regla de negocio."
            };
        }
    }
}
