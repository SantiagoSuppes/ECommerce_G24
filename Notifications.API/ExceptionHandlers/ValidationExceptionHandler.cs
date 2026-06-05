using Microsoft.AspNetCore.Diagnostics;
using System.Net; 
using ECommerce_G24.Notifications.API.Exceptions;

namespace ECommerce_G24.Notifications.API.ExceptionHandlers;

/// <summary>
/// Maneja excepciones de validación.
/// </summary>
public class ValidationExceptionHandler : IExceptionHandler
{
    /// <summary>
    /// Intenta manejar la excepción de validación.
    /// </summary>
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        var correlationId = httpContext.Items["CorrelationId"]?.ToString();

        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.2",
                title = "Bad Request",
                status = 400,
                detail = validationException.Message,
                instance = httpContext.Request.Path,
                errorCode = "NTF-002",
                errorMessage = validationException.Message,
                correlationId
            },
            cancellationToken);

        return true;
    }
}
