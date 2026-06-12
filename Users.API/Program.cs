using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
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

// Nombre del microservicio utilizado en los logs.
var serviceName =
    builder.Configuration["ServiceName"]
    ?? "Users.API";

#region Serilog

// Configuración de logging estructurado.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()

    // Reduce el ruido generado por componentes internos de Microsoft.
    .MinimumLevel.Override(
        "Microsoft",
        LogEventLevel.Warning)

    .MinimumLevel.Override(
        "Microsoft.AspNetCore.Hosting.Diagnostics",
        LogEventLevel.Information)

    // Permite agregar CorrelationId y errorCode.
    .Enrich.FromLogContext()

    // Identifica qué microservicio generó el log.
    .Enrich.WithProperty(
        "Servicio",
        serviceName)

    // Consola en formato legible.
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
        "{Servicio} {Message:lj} " +
        "{Properties:j}{NewLine}{Exception}")

    // Archivo JSON estructurado con rotación diaria.
    .WriteTo.File(
        formatter: new JsonFormatter(),
        path: "logs/users-api-.json",
        rollingInterval: RollingInterval.Day)

    .CreateLogger();

// Reemplaza el logger estándar por Serilog.
builder.Host.UseSerilog();

#endregion

#region Controllers y validaciones

// Registra los controladores.
builder.Services.AddControllers();

// Personaliza las respuestas automáticas de Data Annotations.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // Reúne todos los errores de validación.
        var errors = context.ModelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors)
            .Select(error =>
                string.IsNullOrWhiteSpace(error.ErrorMessage)
                    ? "Valor inválido."
                    : error.ErrorMessage)
            .ToList();

        // El TP permite separar varios errores mediante punto y coma.
        var errorMessage = errors.Count > 0
            ? string.Join("; ", errors)
            : "Los datos del usuario son inválidos.";

        // Recupera el Correlation ID generado por el middleware.
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

                ErrorMessage =
                    errorMessage,

                CorrelationId =
                    correlationId
            });
    };
});

#endregion

#region Swagger

// Descubre los endpoints de los controladores.
builder.Services.AddEndpointsApiExplorer();

// Misma configuración simple utilizada en Products.API y Cart.API.
builder.Services.AddSwaggerGen();

#endregion

#region Dependencias propias

// Inicializador de SQLite.
builder.Services.AddSingleton<DatabaseInitializer>();

// Repositorio de usuarios.
builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

// Servicio de usuarios.
builder.Services.AddScoped<
    IUserService,
    UserService>();

// Servicio de hashing de contraseñas.
builder.Services.AddSingleton<
    IPasswordHasher,
    Pbkdf2PasswordHasher>();

#endregion

#region Correlation ID

// Permite acceder al HttpContext desde handlers HTTP.
builder.Services.AddHttpContextAccessor();

// Handler para propagar el Correlation ID en llamadas salientes.
builder.Services.AddTransient<
    CorrelationIdDelegatingHandler>();

builder.Services
    .AddHttpClient("CorrelatedClient")
    .AddHttpMessageHandler<
        CorrelationIdDelegatingHandler>();

#endregion

#region Exception handlers

// Los handlers específicos se registran antes del handler global.
builder.Services.AddExceptionHandler<
    ValidationExceptionHandler>();

builder.Services.AddExceptionHandler<
    BusinessRuleExceptionHandler>();

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

#endregion

#region Health Checks

// Registra los checks de aplicación y SQLite.
builder.Services
    .AddHealthChecks()

    .AddCheck<ApiStatusCheck>(
        "api-status",
        tags: new[] { "live", "api" })

    .AddCheck<SqliteHealthCheck>(
        "sqlite-db",
        tags: new[] { "ready", "database" });

// Registra el dashboard visual.
builder.Services
    .AddHealthChecksUI(setup =>
    {
        setup.SetEvaluationTimeInSeconds(600);

        setup.AddHealthCheckEndpoint(
            "Users.API",
            "/health");
    })
    .AddInMemoryStorage();

#endregion

var app = builder.Build();

#region Inicialización de SQLite

// Crea la base y la tabla users al iniciar la aplicación.
using (var scope = app.Services.CreateScope())
{
    var initializer =
        scope.ServiceProvider
            .GetRequiredService<DatabaseInitializer>();

    initializer.Initialize();
}

#endregion

#region Swagger

// Misma configuración utilizada en Products.API y Cart.API.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#endregion

// Genera o reutiliza X-Correlation-Id.
app.UseMiddleware<CorrelationIdMiddleware>();

// Registra inicio, fin, estado y duración de cada request.
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
        };
});

// Manejo global de errores mediante IExceptionHandler.
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

// Mapea UsersController.
app.MapControllers();

#region Health endpoints

// Estado general del microservicio.
app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResponseWriter =
            UIResponseWriter.WriteHealthCheckUIResponse
    });

// Readiness: comprueba SQLite.
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate =
            registration =>
                registration.Tags.Contains("ready"),

        ResponseWriter =
            UIResponseWriter.WriteHealthCheckUIResponse
    });

// Liveness: comprueba que la API esté viva.
app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate =
            registration =>
                registration.Tags.Contains("live"),

        ResponseWriter =
            UIResponseWriter.WriteHealthCheckUIResponse
    });

// Dashboard visual.
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
});

#endregion

app.Run();