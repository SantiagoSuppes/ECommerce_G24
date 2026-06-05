using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using ECommerce_G24.Notifications.API.Exceptions;

namespace ECommerce_G24.Notifications.API.ExceptionHandlers;

/// <summary>
/// Intenta manejar la excepción de usuario no encontrado.
/// </summary>
public class UserNotFoundExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Intenta manejar la excepción de usuario no encontrado.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not UserNotFoundException userNotFoundException)
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
                detail = userNotFoundException.Message,
                instance = httpContext.Request.Path,
                errorCode = "NTF-001",
                errorMessage = userNotFoundException.Message,
                correlationId
            },
            cancellationToken);

        return true;
    }
}
