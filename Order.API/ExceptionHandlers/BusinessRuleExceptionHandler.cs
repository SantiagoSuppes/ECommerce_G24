using Microsoft.AspNetCore.Diagnostics;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Serilog.Context;

namespace Orders.API.ExceptionHandlers;

/// <summary>
/// Maneja reglas de negocio 409 y 422.
/// </summary>
public class BusinessRuleExceptionHandler
    : IExceptionHandler
{
    private readonly ILogger<BusinessRuleExceptionHandler> _logger;

    public BusinessRuleExceptionHandler(
        ILogger<BusinessRuleExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException ex)
            return false;

        var metadata =
            GetErrorMetadata(ex.StatusCode);

        context.Items[
            ExceptionHandlerHelper.ErrorCodeItemName] =
            ex.ErrorCode;

        using (LogContext.PushProperty(
                   "errorCode",
                   ex.ErrorCode))
        {
            _logger.LogWarning(
                exception,
                "Regla de negocio incumplida. ErrorCode: {ErrorCode}",
                ex.ErrorCode);
        }

        context.Response.StatusCode =
            ex.StatusCode;

        await context.Response.WriteAsJsonAsync(
            new ErrorResponseDto
            {
                Type = metadata.Type,
                Title = metadata.Title,
                Status = ex.StatusCode,
                Detail = metadata.Detail,
                Instance = context.Request.Path,
                ErrorCode = ex.ErrorCode,
                ErrorMessage = ex.Message,

                CorrelationId =
                    ExceptionHandlerHelper.GetCorrelationId(context)
            },
            cancellationToken);

        return true;
    }

    private static ErrorMetadata GetErrorMetadata(
        int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status422UnprocessableEntity =>
                new ErrorMetadata(
                    "https://tools.ietf.org/html/rfc4918#section-11.2",
                    "Unprocessable Entity",
                    "No se puede procesar la solicitud."),

            StatusCodes.Status409Conflict =>
                new ErrorMetadata(
                    "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    "Conflict",
                    "No se puede modificar el estado."),

            _ =>
                new ErrorMetadata(
                    "about:blank",
                    "Business Rule Error",
                    "No se pudo completar la operación.")
        };
    }

    private sealed record ErrorMetadata(
        string Type,
        string Title,
        string Detail);
}