using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Serilog.Context;

namespace Notifications.API.ExceptionHandlers;

/// <summary>
/// Maneja errores inesperados mediante NTF-004.
/// </summary>
public class GlobalExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IConfiguration _configuration;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        context.Items[
            ExceptionHandlerHelper.ErrorCodeItemName] =
            NotificationErrorCodes.InternalNotificationError;

        using (LogContext.PushProperty(
                   "errorCode",
                   NotificationErrorCodes.InternalNotificationError))
        {
            _logger.LogError(
                exception,
                "Error interno al procesar la notificación.");
        }

        var includeDetails =
            _configuration.GetValue<bool>(
                "ErrorHandling:IncludeExceptionDetails");

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(
            new ErrorResponseDto
            {
                Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.6.1",

                Title =
                    "Internal Server Error",

                Status =
                    StatusCodes.Status500InternalServerError,

                // Nunca se muestra el stack trace.
                Detail = includeDetails
                    ? exception.Message
                    : "Ocurrió un error interno en el servidor.",

                Instance =
                    context.Request.Path,

                ErrorCode =
                    NotificationErrorCodes.InternalNotificationError,

                ErrorMessage =
                    "Error interno al procesar la notificación.",

                CorrelationId =
                    ExceptionHandlerHelper.GetCorrelationId(context)
            },
            cancellationToken);

        return true;
    }
}