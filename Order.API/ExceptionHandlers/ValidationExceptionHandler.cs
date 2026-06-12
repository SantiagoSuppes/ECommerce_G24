using Microsoft.AspNetCore.Diagnostics;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Serilog.Context;

namespace Orders.API.ExceptionHandlers;

/// <summary>
/// Maneja ORD-002.
/// </summary>
public class ValidationExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(
        ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException ex)
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
                "Datos de orden inválidos.");
        }

        context.Response.StatusCode =
            StatusCodes.Status400BadRequest;

        await context.Response.WriteAsJsonAsync(
            new ErrorResponseDto
            {
                Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",

                Title = "Bad Request",

                Status =
                    StatusCodes.Status400BadRequest,

                Detail =
                    "La solicitud contiene datos inválidos.",

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