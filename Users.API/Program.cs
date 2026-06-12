using System.Reflection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Formatting.Json;
using Users.API.Database;
using Users.API.DTOs;
using Users.API.ExceptionHandlers;
using Users.API.Exceptions;
using Users.API.HealthChecks;
using Users.API.Middleware;
using Users.API.Repositories;
using Users.API.Services;

var builder = WebApplication.CreateBuilder(args);

var serviceName =
    builder.Configuration["ServiceName"]
    ?? "Users.API";

// Configuración de Serilog.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()

    // Reduce ruido de logs internos de Microsoft.
    .MinimumLevel.Override(
        "Microsoft",
        LogEventLevel.Warning)

    .MinimumLevel.Override(
        "Microsoft.AspNetCore.Hosting.Diagnostics",
        LogEventLevel.Information)

    // Permite incorporar CorrelationId y errorCode.
    .Enrich.FromLogContext()

    // Identifica el microservicio.
    .Enrich.WithProperty(
        "Servicio",
        serviceName)

    // Consola: solo Error o superior, siguiendo la guía MiniApi.
    .WriteTo.Logger(configuration =>
        configuration
            .Filter.ByIncludingOnly(
                logEvent =>
                    logEvent.Level >= LogEventLevel.Error)
            .WriteTo.Console(
                outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] " +
                "{Servicio} {Message:lj} " +
                "{Properties:j}{NewLine}{Exception}"))

    // Archivo JSON: registra requests HTTP.
    .WriteTo.Logger(configuration =>
        configuration
            .Filter.ByIncludingOnly(logEvent =>
            {
                var isRequestLog =
                    Matching.FromSource(
                        "Serilog.AspNetCore.RequestLoggingMiddleware")
                    (logEvent);

                if (!isRequestLog)
                    return false;

                // Excluye health y swagger para evitar ruido.
                if (logEvent.Properties.TryGetValue(
                        "RequestPath",
                        out var pathProperty) &&
                    pathProperty is ScalarValue scalar &&
                    scalar.Value is string path)
                {
                    return !path.Contains("/health") &&
                           !path.Contains("/swagger");
                }

                return true;
            })
            .WriteTo.File(
                new JsonFormatter(),
                "logs/users-api-.json",
                rollingInterval:
                    RollingInterval.Day))

    .CreateLogger();

builder.Host.UseSerilog();

// Controladores.
builder.Services.AddControllers();

// Personaliza los errores automáticos de Data Annotations.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(entry =>
                entry.Value?.Errors.Count > 0)
            .SelectMany(entry =>
                entry.Value!.Errors)
            .Select(error =>
                string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Valor inválido."
                    : error.ErrorMessage)
            .ToList();

        var errorMessage =
            errors.Count > 0
                ? string.Join("; ", errors)
                : "Los datos del usuario son inválidos.";

        // Como el catálogo no define otro código de validación
        // para login, se utiliza USR-002 para requests inválidos.
        context.HttpContext.Items[
            ExceptionHandlerHelper.ErrorCodeItemName] =
            UserErrorCodes.InvalidUserData;

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
                Title = "Bad Request",
                Status =
                    StatusCodes.Status400BadRequest,
                Detail =
                    "La solicitud contiene datos inválidos.",
                Instance =
                    context.HttpContext.Request.Path,
                ErrorCode =
                    UserErrorCodes.InvalidUserData,
                ErrorMessage = errorMessage,
                CorrelationId = correlationId
            });
    };
});

// Swagger.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "Users.API",
            Version = "v1",
            Description =
                "Microservicio de usuarios del sistema E-Commerce."
        });

    // Incluye XML comments.
    var xmlFile =
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath =
        Path.Combine(
            AppContext.BaseDirectory,
            xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Inicialización de base.
builder.Services.AddSingleton<DatabaseInitializer>();

// Repositorio.
builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

// Servicios.
builder.Services.AddScoped<
    IUserService,
    UserService>();

builder.Services.AddSingleton<
    IPasswordHasher,
    Pbkdf2PasswordHasher>();

// Correlation ID para HTTP saliente.
builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<
    CorrelationIdDelegatingHandler>();

builder.Services
    .AddHttpClient("CorrelatedClient")
    .AddHttpMessageHandler<
        CorrelationIdDelegatingHandler>();

// Exception handlers.
// Los específicos deben registrarse antes que el global.
builder.Services.AddExceptionHandler<
    ValidationExceptionHandler>();

builder.Services.AddExceptionHandler<
    BusinessRuleExceptionHandler>();

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

// Health Checks.
builder.Services
    .AddHealthChecks()
    .AddCheck<ApiStatusCheck>(
        "api-status",
        tags: new[] { "live", "api" })
    .AddCheck<SqliteHealthCheck>(
        "sqlite-db",
        tags: new[] { "ready", "database" });

// Dashboard visual de Health Checks.
builder.Services
    .AddHealthChecksUI(setup =>
    {
        setup.SetEvaluationTimeInSeconds(600);

        setup.AddHealthCheckEndpoint(
            "Users.API",
            "/health");
    })
    .AddInMemoryStorage();

var app = builder.Build();

// Inicializa la base al arrancar.
using (var scope = app.Services.CreateScope())
{
    var initializer =
        scope.ServiceProvider
            .GetRequiredService<DatabaseInitializer>();

    initializer.Initialize();
}

// Swagger solo en Development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Genera el Correlation ID.
app.UseMiddleware<CorrelationIdMiddleware>();

// Log de inicio, fin, estado y duración de cada request.
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

// Manejo global con IExceptionHandler.
// No se utiliza middleware personalizado para manejar errores.
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Estado general.
app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResponseWriter =
            UIResponseWriter
                .WriteHealthCheckUIResponse
    });

// Readiness: incluye SQLite.
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

// Liveness: solo verifica que la API esté viva.
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

// Dashboard visual de Health Checks.
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});

app.Run();