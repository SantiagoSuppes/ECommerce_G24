using Users.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace Users.API.ExceptionHandlers;

public class UserBlockedExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not UserBlockedException userBlockedException)
            return false;

        var correlationId = httpContext.Items["CorrelationId"]?.ToString();

        httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                title = "Forbidden",
                status = 403,
                detail = userBlockedException.Message,
                instance = httpContext.Request.Path,
                errorCode = UserBlockedException.ErrorCode,
                errorMessage = userBlockedException.Message,
                correlationId
            },
            cancellationToken);

        return true;
    }
}
