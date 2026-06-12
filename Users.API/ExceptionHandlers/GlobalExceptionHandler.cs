using Microsoft.AspNetCore.Diagnostics;
using Serilog.Context;
using Users.API.DTOs;
using Users.API.Exceptions;

namespace Users.API.ExceptionHandlers;

/// <summary>
/// Maneja errores inesperados y devuelve USR-006.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
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
            UserErrorCodes.InternalUserError;

        using (LogContext.PushProperty(
                   "errorCode",
                   UserErrorCodes.InternalUserError))
        {
            _logger.LogError(
                exception,
                "Error interno al procesar el usuario.");
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
                Title = "Internal Server Error",
                Status =
                    StatusCodes.Status500InternalServerError,

                // En desarrollo se muestra el mensaje,
                // pero nunca el stack trace.
                Detail = includeDetails
                    ? exception.Message
                    : "Ocurrió un error interno en el servidor.",

                Instance = context.Request.Path,
                ErrorCode =
                    UserErrorCodes.InternalUserError,
                ErrorMessage =
                    "Error interno al procesar el usuario.",
                CorrelationId =
                    ExceptionHandlerHelper.GetCorrelationId(context)
            },
            cancellationToken);

        return true;
    }
}