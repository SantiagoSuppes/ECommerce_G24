using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using ECommerce_G24.src.Orders.API.Exceptions;

namespace ECommerce_G24.src.Orders.API.ExceptionHandlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

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
                errorCode = "ORD-002",
                errorMessage = validationException.Message
            },
            cancellationToken);

        return true;
    }
}
