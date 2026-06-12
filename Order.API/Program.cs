using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Orders.API.Clients;
using Orders.API.Database;
using Orders.API.DTOs;
using Orders.API.ExceptionHandlers;
using Orders.API.Exceptions;
using Orders.API.HealthChecks;
using Orders.API.Middleware;
using Orders.API.Repositories;
using Orders.API.Services;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);

var serviceName =
    builder.Configuration["ServiceName"]
    ?? "Orders.API";

#region Serilog

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()

    .MinimumLevel.Override(
        "Microsoft",
        LogEventLevel.Warning)

    .MinimumLevel.Override(
        "Microsoft.AspNetCore.Hosting.Diagnostics",
        LogEventLevel.Information)

    // Permite agregar propiedades desde el contexto.
    .Enrich.FromLogContext()

    // Identifica el microservicio.
    .Enrich.WithProperty(
        "Servicio",
        serviceName)

    // Consola en formato legible.
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
        "{Servicio} {Message:lj} " +
        "{Properties:j}{NewLine}{Exception}")

 
    .WriteTo.Logger(configuration =>
        configuration
            .Filter.ByExcluding(logEvent =>
            {
                var isRequestLog =
                    Matching.FromSource(
                        "Serilog.AspNetCore.RequestLoggingMiddleware")
                    (logEvent);

                if (!isRequestLog)
                    return false;

                if (logEvent.Properties.TryGetValue(
                        "RequestPath",
                        out var pathProperty) &&
                    pathProperty is ScalarValue scalar &&
                    scalar.Value is string path)
                {
                    return path.Contains("/health") ||
                           path.Contains("/swagger");
                }

                return false;
            })
            .WriteTo.File(
                formatter:
                    new JsonFormatter(),

                path:
                    "logs/orders-api-.json",

                rollingInterval:
                    RollingInterval.Day))

    .CreateLogger();

builder.Host.UseSerilog();

#endregion

#region Controllers y validación

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors =
            context.ModelState
                .Where(entry =>
                    entry.Value?.Errors.Count > 0)
                .SelectMany(entry =>
                    entry.Value!.Errors)
                .Select(error =>
                    string.IsNullOrWhiteSpace(
                        error.ErrorMessage)
                        ? "Valor inválido."
                        : error.ErrorMessage)
                .ToList();

        var errorMessage =
            errors.Count > 0
                ? string.Join("; ", errors)
                : "Los datos de la orden son inválidos.";

        context.HttpContext.Items[
            ExceptionHandlerHelper.ErrorCodeItemName] =
            OrderErrorCodes.InvalidOrderData;

        var correlationId =
            context.HttpContext.Items.TryGetValue(
                CorrelationIdMiddleware.HeaderName,
                out var value)
                ? value?.ToString()
                  ?? context.HttpContext.TraceIdentifier
                : context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(
            new ErrorResponseDto
            {
                Type =
                    "https://tools.ietf.org/html/rfc7231#section-6.5.1",

                Title =
                    "Bad Request",

                Status =
                    StatusCodes.Status400BadRequest,

                Detail =
                    "La solicitud contiene datos inválidos.",

                Instance =
                    context.HttpContext.Request.Path,

                ErrorCode =
                    OrderErrorCodes.InvalidOrderData,

                ErrorMessage =
                    errorMessage,

                CorrelationId =
                    correlationId
            });
    };
});

#endregion

#region Swagger


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

#endregion

#region Dependencias

builder.Services.AddSingleton<
    DatabaseInitializer>();

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

builder.Services.AddScoped<
    IOrderService,
    OrderService>();

#endregion

#region Clientes HTTP

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<
    CorrelationIdDelegatingHandler>();

// Cliente hacia Users.API.
builder.Services
    .AddHttpClient<IUsersApiClient, UsersApiClient>(
        (serviceProvider, client) =>
        {
            var configuration =
                serviceProvider.GetRequiredService<
                    IConfiguration>();

            var baseUrl =
                configuration[
                    "ExternalServices:UsersApi:BaseUrl"]
                ?? "https://localhost:7003";

            client.BaseAddress =
                new Uri(baseUrl);
        })
    .AddHttpMessageHandler<
        CorrelationIdDelegatingHandler>();

// Cliente hacia Products.API.
builder.Services
    .AddHttpClient<IProductsApiClient, ProductsApiClient>(
        (serviceProvider, client) =>
        {
            var configuration =
                serviceProvider.GetRequiredService<
                    IConfiguration>();

            var baseUrl =
                configuration[
                    "ExternalServices:ProductsApi:BaseUrl"]
                ?? "https://localhost:7001";

            client.BaseAddress =
                new Uri(baseUrl);
        })
    .AddHttpMessageHandler<
        CorrelationIdDelegatingHandler>();

#endregion

#region Exception handlers

builder.Services.AddExceptionHandler<
    NotFoundExceptionHandler>();

builder.Services.AddExceptionHandler<
    ValidationExceptionHandler>();

builder.Services.AddExceptionHandler<
    BusinessRuleExceptionHandler>();

builder.Services.AddExceptionHandler<
    InternalServerExceptionHandler>();
builder.Services.AddExceptionHandler<
    UnprocessableEntityExceptionHandler>();

builder.Services.AddProblemDetails();

#endregion

#region Health Checks

builder.Services
    .AddHealthChecks()

    .AddCheck<ApiStatusCheck>(
        "api-status",
        tags: new[]
        {
            "live",
            "api"
        })

    .AddCheck<SqliteHealthCheck>(
        "sqlite-db",
        tags: new[]
        {
            "ready",
            "database"
        });

builder.Services
    .AddHealthChecksUI(setup =>
    {
        setup.SetEvaluationTimeInSeconds(600);

        setup.AddHealthCheckEndpoint(
            "Orders.API",
            "/health");
    })
    .AddInMemoryStorage();

#endregion

var app = builder.Build();

#region Inicialización SQLite

using (var scope = app.Services.CreateScope())
{
    var initializer =
        scope.ServiceProvider
            .GetRequiredService<
                DatabaseInitializer>();

    initializer.Initialize();
}

#endregion

#region Swagger

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#endregion

// Genera el Correlation ID.
app.UseMiddleware<
    CorrelationIdMiddleware>();

// Registra requests con estado y duración.
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel =
        (httpContext, elapsed, exception) =>
        {
            if (exception is not null)
                return LogEventLevel.Error;

            if (httpContext.Response.StatusCode >= 500)
                return LogEventLevel.Error;

            if (httpContext.Response.StatusCode >= 400)
                return LogEventLevel.Warning;

            if (httpContext.Request.Path
                .StartsWithSegments("/health"))
            {
                return LogEventLevel.Verbose;
            }

            return LogEventLevel.Information;
        };

    options.EnrichDiagnosticContext =
        (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set(
                "Endpoint",
                httpContext.Request.Path);

            diagnosticContext.Set(
                "RequestMethod",
                httpContext.Request.Method);

            diagnosticContext.Set(
                "StatusCode",
                httpContext.Response.StatusCode);

            if (httpContext.Items.TryGetValue(
                    CorrelationIdMiddleware.HeaderName,
                    out var correlationId))
            {
                diagnosticContext.Set(
                    "CorrelationId",
                    correlationId);
            }

            if (httpContext.Items.TryGetValue(
                    ExceptionHandlerHelper.ErrorCodeItemName,
                    out var errorCode))
            {
                diagnosticContext.Set(
                    "errorCode",
                    errorCode);
            }
        };
});

// Manejo global de errores.
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

#region Health endpoints

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResponseWriter =
            UIResponseWriter
                .WriteHealthCheckUIResponse
    });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate =
            registration =>
                registration.Tags.Contains("ready"),

        ResponseWriter =
            UIResponseWriter
                .WriteHealthCheckUIResponse
    });

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate =
            registration =>
                registration.Tags.Contains("live"),

        ResponseWriter =
            UIResponseWriter
                .WriteHealthCheckUIResponse
    });

app.MapHealthChecksUI(options =>
{
    options.UIPath =
        "/health-ui";
});

#endregion

app.Run();