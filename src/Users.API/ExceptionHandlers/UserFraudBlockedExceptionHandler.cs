using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using ECommerce_G24.source.Users.API.Exceptions;

namespace ECommerce_G24.source.Users.API.ExceptionHandlers;

public class UserFraudBlockedExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not UserFraudBlockedException)
            return false;

        httpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                title = "Forbidden",
                status = 403,
                detail = UserFraudBlockedException.ErrorMessage,
                instance = httpContext.Request.Path,
                errorCode = UserFraudBlockedException.ErrorCode,
                errorMessage = UserFraudBlockedException.ErrorMessage
            },
            cancellationToken);

        return true;
    }
}
