using ECommerce_G24.Products.API.Database;
using ECommerce_G24.Products.API.ExceptionHandlers;
using ECommerce_G24.Products.API.HealthChecks;
using ECommerce_G24.Products.API.Middleware;
using ECommerce_G24.Products.API.Repositories;
using ECommerce_G24.Products.API.Services;
using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Exceptions;
using ECommerce_G24.Products.API.Models;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using Serilog;
using Serilog.Events;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Nombre del servicio para logs estructurados.
var serviceName = builder.Configuration["ServiceName"] ?? "Products.API";

// Configuración de Serilog.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", serviceName)

    // Consola con formato legible.
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Servicio} {Message:lj} {Properties:j}{NewLine}{Exception}")

    // Archivo JSON estructurado con rotación diaria.
    .WriteTo.File(
        path: "logs/products-api-.json",
        rollingInterval: RollingInterval.Day,
        formatter: new Serilog.Formatting.Json.JsonFormatter())

    .CreateLogger();

builder.Host.UseSerilog();

// Agrega controladores.
builder.Services.AddControllers();

// Configura respuestas automáticas de validación para respetar el contrato de error PRD-002.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        // Junta todos los errores de validación separados por punto y coma,
     
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        var errorMessage = errors.Count > 0
            ? string.Join("; ", errors)
            : "Los datos del producto son inválidos.";

        var correlationId = context.HttpContext.Items.TryGetValue("X-Correlation-Id", out var value)
            ? value?.ToString() ?? context.HttpContext.TraceIdentifier
            : context.HttpContext.TraceIdentifier;

        var response = new ErrorResponseDto
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Status = StatusCodes.Status400BadRequest,
            Detail = "La solicitud contiene datos inválidos.",
            Instance = context.HttpContext.Request.Path,
            ErrorCode = ProductErrorCodes.InvalidProductData,
            ErrorMessage = errorMessage,
            CorrelationId = correlationId
        };

        return new BadRequestObjectResult(response);
    };
});

// Swagger / OpenAPI.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Products.API",
        Version = "v1",
        Description = "Microservicio de productos para el sistema E-Commerce."
    });

    // Incluye XML comments en Swagger.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// Registro de dependencias de persistencia.
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Registro de servicios de negocio.
builder.Services.AddScoped<IProductService, ProductService>();

// Registro de ExceptionHandlers.
// El orden importa: primero específicos, último el genérico.
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Health Checks.
builder.Services.AddHealthChecks()
    .AddCheck<ApiStatusCheck>("api-status", tags: new[] { "live" })
    .AddCheck<SqliteHealthCheck>("sqlite-db", tags: new[] { "ready", "database" });

// Dashboard UI de Health Checks
builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(600);
    setup.AddHealthCheckEndpoint("Products.API", "/health");
})
.AddInMemoryStorage();

// IHttpClientFactory queda registrado para futuras llamadas HTTP entre microservicios.
builder.Services.AddHttpClient();

var app = builder.Build();

// Inicializa la base de datos al arrancar.
using (var scope = app.Services.CreateScope())
{
    var databaseInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    databaseInitializer.Initialize();
}

// Swagger en ambiente Development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware de Correlation ID.
app.UseMiddleware<CorrelationIdMiddleware>();

// Logging automático de requests con Serilog.
app.UseSerilogRequestLogging(options =>
{
    // Define nivel de log por request.
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex is not null)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 400)
            return LogEventLevel.Warning;

        if (httpContext.Request.Path.StartsWithSegments("/health"))
            return LogEventLevel.Verbose;

        return LogEventLevel.Information;
    };

    // Agrega propiedades útiles a cada log de request.
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("Endpoint", httpContext.Request.Path);
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
        diagnosticContext.Set("StatusCode", httpContext.Response.StatusCode);

        if (httpContext.Items.TryGetValue("X-Correlation-Id", out var correlationId))
            diagnosticContext.Set("CorrelationId", correlationId);
    };
});

// Manejo global de errores con IExceptionHandler.
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Health Check general.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Readiness: valida dependencias necesarias, como SQLite.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Liveness: valida que la API esté viva.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Dashboard visual de Health Checks.
app.MapHealthChecksUI(setup =>
{
    setup.UIPath = "/health-ui";
});

app.Run();