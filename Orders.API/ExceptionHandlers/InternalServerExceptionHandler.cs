using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using ECommerce_G24.Orders.API.Exceptions;

namespace ECommerce_G24.Orders.API.ExceptionHandlers;

public class InternalServerExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not InternalServerException internalServerException)
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
                detail = internalServerException.Message,
                instance = httpContext.Request.Path,
                errorCode = "ORD-007",
                errorMessage = internalServerException.Message,
                correlationId
            },
            cancellationToken);

        return true;
    }
}
