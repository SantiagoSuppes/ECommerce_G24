using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
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

// Nombre del microservicio utilizado en los logs.
var serviceName =
    builder.Configuration["ServiceName"]
    ?? "Orders.API";

#region Serilog

/*
 * Configuración de logging estructurado.
 *
 * - Consola con formato legible.
 * - Archivo JSON con rotación diaria.
 * - Correlation ID incorporado mediante LogContext.
 * - Identificación del servicio que genera el log.
 */
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()

    // Reduce el ruido generado por componentes internos.
    .MinimumLevel.Override(
        "Microsoft",
        LogEventLevel.Warning)

    .MinimumLevel.Override(
        "Microsoft.AspNetCore.Hosting.Diagnostics",
        LogEventLevel.Information)

    // Permite agregar CorrelationId y errorCode.
    .Enrich.FromLogContext()

    // Identifica el microservicio.
    .Enrich.WithProperty(
        "Servicio",
        serviceName)

    // Salida legible por consola.
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
        "{Servicio} {Message:lj} " +
        "{Properties:j}{NewLine}{Exception}")

    /*
     * Archivo JSON estructurado.
     *
     * Se excluyen únicamente los logs HTTP de:
     * - /health
     * - /swagger
     *
     * Los logs internos de negocio y excepciones
     * siguen registrándose.
     */
    .WriteTo.Logger(configuration =>
        configuration
            .Filter.ByExcluding(logEvent =>
            {
                var isRequestLog =
                    Matching.FromSource(
                        "Serilog.AspNetCore.RequestLoggingMiddleware")
                    (logEvent);

                // Si no es un log HTTP, no se excluye.
                if (!isRequestLog)
                    return false;

                if (logEvent.Properties.TryGetValue(
                        "RequestPath",
                        out var pathProperty) &&
                    pathProperty is ScalarValue scalar &&
                    scalar.Value is string path)
                {
                    return path.Contains(
                               "/health",
                               StringComparison.OrdinalIgnoreCase) ||
                           path.Contains(
                               "/swagger",
                               StringComparison.OrdinalIgnoreCase);
                }

                return false;
            })
            .WriteTo.File(
                formatter: new JsonFormatter(),
                path: "logs/orders-api-.json",
                rollingInterval: RollingInterval.Day))

    .CreateLogger();

// Reemplaza el sistema de logging estándar por Serilog.
builder.Host.UseSerilog();

#endregion

#region Controllers y validación automática

// Registra los controladores de la API.
builder.Services.AddControllers();

// Registra los servicios básicos de autorización.
// Aunque todavía no haya autenticación, UseAuthorization
// necesita esta configuración.
builder.Services.AddAuthorization();

/*
 * Personaliza las respuestas automáticas generadas
 * por Data Annotations y [ApiController].
 *
 * Todos los errores de validación de Orders.API
 * se devuelven con ORD-002.
 */
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

        // Guarda el código para incorporarlo al log HTTP.
        context.HttpContext.Items[
            ExceptionHandlerHelper.ErrorCodeItemName] =
            OrderErrorCodes.InvalidOrderData;

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
                    "https://tools.ietf.org/html/" +
                    "rfc7231#section-6.5.1",

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

/*
 * Configuración simple de Swagger.
 *
 * Esta es la misma configuración que funcionó
 * en Products.API, Cart.API y Users.API.
 */
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#endregion

#region Persistencia y servicios de negocio

// Inicializa la base SQLite al arrancar.
builder.Services.AddSingleton<
    DatabaseInitializer>();

// Repositorio SQLite + Dapper.
builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

// Lógica de negocio de órdenes.
builder.Services.AddScoped<
    IOrderService,
    OrderService>();

#endregion

#region Clientes HTTP y Correlation ID

/*
 * Permite que el DelegatingHandler acceda
 * al HttpContext del request actual.
 */
builder.Services.AddHttpContextAccessor();

/*
 * Handler utilizado para propagar X-Correlation-Id
 * en llamadas salientes.
 */
builder.Services.AddTransient<
    CorrelationIdDelegatingHandler>();

/*
 * Cliente HTTP hacia Users.API.
 *
 * Orders.API lo utiliza para consultar:
 * GET /api/users/{id}/exists
 */
builder.Services
    .AddHttpClient<IUsersApiClient, UsersApiClient>(
        (serviceProvider, client) =>
        {
            var configuration =
                serviceProvider
                    .GetRequiredService<IConfiguration>();

            var baseUrl =
                configuration[
                    "ExternalServices:UsersApi:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "No se configuró " +
                    "ExternalServices:UsersApi:BaseUrl.");

            client.BaseAddress =
                new Uri(baseUrl);

            // Evita que una llamada quede esperando indefinidamente.
            client.Timeout =
                TimeSpan.FromSeconds(10);
        })
    .AddHttpMessageHandler<
        CorrelationIdDelegatingHandler>();

/*
 * Cliente HTTP hacia Products.API.
 *
 * Orders.API lo utiliza para consultar:
 * GET /api/products/{id}
 */
builder.Services
    .AddHttpClient<IProductsApiClient, ProductsApiClient>(
        (serviceProvider, client) =>
        {
            var configuration =
                serviceProvider
                    .GetRequiredService<IConfiguration>();

            var baseUrl =
                configuration[
                    "ExternalServices:ProductsApi:BaseUrl"]
                ?? throw new InvalidOperationException(
                    "No se configuró " +
                    "ExternalServices:ProductsApi:BaseUrl.");

            client.BaseAddress =
                new Uri(baseUrl);

            client.Timeout =
                TimeSpan.FromSeconds(10);
        })
    .AddHttpMessageHandler<
        CorrelationIdDelegatingHandler>();

#endregion

#region Exception Handlers

/*
 * El orden es importante.
 *
 * Primero se registran los handlers específicos.
 * El handler general de error 500 queda último,
 * para no interceptar errores 400, 404, 409 o 422.
 */

// ORD-002: datos de orden inválidos.
builder.Services.AddExceptionHandler<
    ValidationExceptionHandler>();

// ORD-001, ORD-003 y ORD-004: recursos no encontrados.
builder.Services.AddExceptionHandler<
    NotFoundExceptionHandler>();

// ORD-005: stock insuficiente.
builder.Services.AddExceptionHandler<
    UnprocessableEntityExceptionHandler>();

// ORD-006: transición de estado inválida.
builder.Services.AddExceptionHandler<
    BusinessRuleExceptionHandler>();

// ORD-007: cualquier error inesperado.
// Siempre debe registrarse al final.
builder.Services.AddExceptionHandler<
    InternalServerExceptionHandler>();

// Infraestructura necesaria para UseExceptionHandler.
builder.Services.AddProblemDetails();

#endregion

#region Health Checks

/*
 * api-status:
 * Comprueba que la aplicación esté viva.
 *
 * sqlite-db:
 * Comprueba que SQLite esté disponible.
 */
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

/*
 * Dashboard visual de Health Checks.
 *
 * Requiere:
 * AspNetCore.HealthChecks.UI.InMemory.Storage
 */
builder.Services
    .AddHealthChecksUI(setup =>
    {
        // Evalúa el estado cada 10 minutos.
        setup.SetEvaluationTimeInSeconds(600);

        setup.AddHealthCheckEndpoint(
            "Orders.API",
            "/health");
    })
    .AddInMemoryStorage();

#endregion

var app = builder.Build();

#region Inicialización de SQLite

/*
 * Crea la base y las tablas de Orders.API
 * antes de aceptar requests.
 */
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

/*
 * Swagger solamente se expone
 * en el ambiente Development.
 */
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

#endregion

#region Pipeline HTTP

/*
 * Genera o reutiliza X-Correlation-Id.
 *
 * Se coloca antes del logging y del manejo
 * de errores para que todos puedan accederlo.
 */
app.UseMiddleware<
    CorrelationIdMiddleware>();

/*
 * Registra una entrada por request con:
 * - endpoint;
 * - método HTTP;
 * - estado;
 * - duración;
 * - Correlation ID;
 * - errorCode cuando corresponda.
 */
app.UseSerilogRequestLogging(options =>
{
    options.GetLevel =
        (httpContext, elapsed, exception) =>
        {
            // Excepción inesperada.
            if (exception is not null)
                return LogEventLevel.Error;

            // Respuesta 5xx.
            if (httpContext.Response.StatusCode >= 500)
                return LogEventLevel.Error;

            // Respuesta 4xx.
            if (httpContext.Response.StatusCode >= 400)
                return LogEventLevel.Warning;

            // Evita ruido de Health Checks.
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

/*
 * Activa los IExceptionHandler registrados.
 *
 * El TP exige usar app.UseExceptionHandler()
 * y no un middleware personalizado para errores.
 */
app.UseExceptionHandler();

// Redirige HTTP a HTTPS para requests externos.
app.UseHttpsRedirection();

// Activa autorización.
app.UseAuthorization();

// Expone los controladores.
app.MapControllers();

#endregion

#region Endpoints de Health Checks

/*
 * Estado general.
 *
 * Ejecuta todos los checks.
 */
app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResponseWriter =
            UIResponseWriter
                .WriteHealthCheckUIResponse
    });

/*
 * Readiness.
 *
 * Comprueba que la API esté lista para trabajar,
 * incluyendo la disponibilidad de SQLite.
 */
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

/*
 * Liveness.
 *
 * Comprueba solamente que la API esté viva.
 */
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

/*
 * Dashboard visual.
 */
app.MapHealthChecksUI(options =>
{
    options.UIPath =
        "/health-ui";
});

#endregion

app.Run();