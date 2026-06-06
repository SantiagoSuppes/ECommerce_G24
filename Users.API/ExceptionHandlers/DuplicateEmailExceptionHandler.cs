using Users.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace Users.API.ExceptionHandlers;

public class DuplicateEmailExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DuplicateEmailException)
            return false;

        var correlationId = httpContext.Items["CorrelationId"]?.ToString();

        httpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
                title = "Conflict",
                status = 409,
                detail = DuplicateEmailException.ErrorMessage,
                instance = httpContext.Request.Path,
                errorCode = DuplicateEmailException.ErrorCode,
                errorMessage = DuplicateEmailException.ErrorMessage,
                correlationId
            },
            cancellationToken);

        return true;
    }
}
