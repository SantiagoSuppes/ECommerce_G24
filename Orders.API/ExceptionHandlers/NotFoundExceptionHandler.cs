using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using ECommerce_G24.Orders.API.Exceptions;

namespace ECommerce_G24.Orders.API.ExceptionHandlers;

public class NotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundException)
            return false;

        var correlationId = httpContext.Items["CorrelationId"]?.ToString();

        httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = notFoundException.Message,
                instance = httpContext.Request.Path,
                errorCode = "ORD-001",
                errorMessage = notFoundException.Message,
                correlationId
            },
            cancellationToken);

        return true;
    }
}
