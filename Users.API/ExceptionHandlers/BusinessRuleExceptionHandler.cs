using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;
using Users.API.DTOs;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

/// <summary>
/// Maneja errores 401, 403 y 409 de Users.API.
/// </summary>
public class BusinessRuleExceptionHandler : IExceptionHandler
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

        var metadata = GetMetadata(ex.StatusCode);

        context.Items[
            ExceptionHandlerHelper.ErrorCodeItemName] =
            ex.ErrorCode;

        using (LogContext.PushProperty(
                   "errorCode",
                   ex.ErrorCode))
        {
            _logger.LogWarning(
                exception,
                "Regla de negocio de usuario incumplida. ErrorCode: {ErrorCode}",
                ex.ErrorCode);
        }

        context.Response.StatusCode = ex.StatusCode;

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

    private static ErrorMetadata GetMetadata(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status401Unauthorized =>
                new ErrorMetadata(
                    "https://tools.ietf.org/html/rfc7235#section-3.1",
                    "Unauthorized",
                    "Las credenciales no son válidas."),

            StatusCodes.Status403Forbidden =>
                new ErrorMetadata(
                    "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                    "Forbidden",
                    "El acceso está prohibido."),

            StatusCodes.Status409Conflict =>
                new ErrorMetadata(
                    "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                    "Conflict",
                    "Ya existe un recurso con esos datos."),

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