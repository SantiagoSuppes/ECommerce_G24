using Microsoft.AspNetCore.Diagnostics;
using Orders.API.Exceptions;

namespace Orders.API.ExceptionHandlers;

public class UnprocessableEntityExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not UnprocessableEntityException ex)
            return false;

        var correlationId = httpContext.Items["X-Correlation-Id"]?.ToString();

        httpContext.Response.StatusCode = 422;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc4918#section-11.2",
            title = "Unprocessable Entity",
            status = 422,
            detail = ex.Message,
            instance = httpContext.Request.Path,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message,
            correlationId
        }, cancellationToken);

        return true;
    }
}