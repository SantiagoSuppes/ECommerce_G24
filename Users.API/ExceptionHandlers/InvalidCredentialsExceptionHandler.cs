using Users.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace Users.API.ExceptionHandlers;

public class InvalidCredentialsExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not InvalidCredentialsException)
            return false;

        httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                title = "Unauthorised",
                status = 401,
                detail = InvalidCredentialsException.ErrorMessage,
                instance = httpContext.Request.Path,
                errorCode = InvalidCredentialsException.ErrorCode,
                errorMessage = InvalidCredentialsException.ErrorMessage
            },
            cancellationToken);

        return true;
    }
}
