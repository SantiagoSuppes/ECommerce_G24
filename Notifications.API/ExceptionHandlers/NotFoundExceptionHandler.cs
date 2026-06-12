using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Serilog.Context;

namespace Notifications.API.ExceptionHandlers;

/// <summary>
/// Maneja NTF-001 y NTF-003.
/// </summary>
public class NotFoundExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    public NotFoundExceptionHandler(
        ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException ex)
            return false;

        context.Items[
            ExceptionHandlerHelper.ErrorCodeItemName] =
            ex.ErrorCode;

        using (LogContext.PushProperty(
                   "errorCode",
                   ex.ErrorCode))
        {
            _logger.LogWarning(
                exception,
                "Recurso no encontrado. ErrorCode: {ErrorCode}",
                ex.ErrorCode);
        }

        context.Response.StatusCode =
            StatusCodes.Status404NotFound;

        await context.Response.WriteAsJsonAsync(
            new ErrorResponseDto
            {
                Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.4",

                Title = "Not Found",

                Status =
                    StatusCodes.Status404NotFound,

                Detail =
                    "El recurso solicitado no fue encontrado.",

                Instance =
                    context.Request.Path,

                ErrorCode =
                    ex.ErrorCode,

                ErrorMessage =
                    ex.Message,

                CorrelationId =
                    ExceptionHandlerHelper.GetCorrelationId(context)
            },
            cancellationToken);

        return true;
    }
}