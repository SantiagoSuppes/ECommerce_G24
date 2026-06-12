using Microsoft.AspNetCore.Diagnostics;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Serilog.Context;
using System.Net;

namespace Orders.API.ExceptionHandlers;

public class InternalServerExceptionHandler : IExceptionHandler
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
            OrderErrorCodes.InternalOrderError;

        using (LogContext.PushProperty(
                   "errorCode",
                   OrderErrorCodes.InternalOrderError))
        {
            _logger.LogError(
                exception,
                "Error interno al procesar la orden.");
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

                // Nunca se devuelve el stack trace.
                Detail = includeDetails
                    ? exception.Message
                    : "Ocurrió un error interno en el servidor.",

                Instance =
                    context.Request.Path,

                ErrorCode =
                    OrderErrorCodes.InternalOrderError,

                ErrorMessage =
                    "Error interno al procesar la orden.",

                CorrelationId =
                    ExceptionHandlerHelper.GetCorrelationId(context)
            },
            cancellationToken);

        return true;
    }
}