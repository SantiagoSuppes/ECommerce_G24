using Users.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace Users.API.ExceptionHandlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        var correlationId = httpContext.Items["CorrelationId"]?.ToString();

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Bad Request",
                status = 400,
                detail = validationException.Message,
                instance = httpContext.Request.Path,
                errorCode = ValidationException.ErrorCode,
                errorMessage = validationException.Message,
                correlationId
            },
            cancellationToken);

        return true;
    }
}
