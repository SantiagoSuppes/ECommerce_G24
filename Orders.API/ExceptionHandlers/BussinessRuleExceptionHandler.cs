using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using ECommerce_G24.Orders.API.Exceptions;

namespace ECommerce_G24.Orders.API.ExceptionHandlers;

public class BussinessRuleExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BussinessRuleException bussinessRuleException)
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
                detail = bussinessRuleException.Message,
                instance = httpContext.Request.Path,
                errorCode = "ORD-003",
                errorMessage = bussinessRuleException.Message,
                correlationId
            },
            cancellationToken);

        return true;
    }
}
