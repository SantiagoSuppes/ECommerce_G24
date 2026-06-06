using Users.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace Users.API.ExceptionHandlers;

public class InternalServerExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not InternalServerException)
            return false;

        var correlationId = httpContext.Items["CorrelationId"]?.ToString();

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = InternalServerException.ErrorMessage,
                instance = httpContext.Request.Path,
                errorCode = InternalServerException.ErrorCode,
                errorMessage = InternalServerException.ErrorMessage,
                correlationId
            },
            cancellationToken);

        return true;
    }
}
