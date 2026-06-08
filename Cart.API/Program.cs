using ECommerce_G24.Cart.API.Database;
using ECommerce_G24.Cart.API.ExceptionHandlers;
using ECommerce_G24.Cart.API.Dtos;
using ECommerce_G24.Cart.API.Exceptions;
using ECommerce_G24.Cart.API.HealthChecks;
using ECommerce_G24.Cart.API.Middleware;
using ECommerce_G24.Cart.API.Repositories;
using ECommerce_G24.Cart.API.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Nombre del servicio para logs.
var serviceName = builder.Configuration["ServiceName"] ?? "Cart.API";

// Configuración de Serilog.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Servicio", serviceName)

    // Consola en formato legible.
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Servicio} {Message:lj} {Properties:j}{NewLine}{Exception}")

    // Archivo JSON estructurado con rotación diaria.
    .WriteTo.File(
        path: "logs/cart-api-.json",
        rollingInterval: RollingInterval.Day,
        formatter: new Serilog.Formatting.Json.JsonFormatter())

    .CreateLogger();

builder.Host.UseSerilog();

// Controladores.
builder.Services.AddControllers();

// Personaliza errores de validación automática para que usen CRT-004.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToList();

        var errorMessage = errors.Count > 0
            ? string.Join("; ", errors)
            : "Cantidad inválida.";

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
            ErrorCode = CartErrorCodes.InvalidQuantity,
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
        Title = "Cart.API",
        Version = "v1",
        Description = "Microservicio de carrito para el sistema E-Commerce."
    });

    // Incluye XML comments en Swagger.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// Persistencia.
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddScoped<ICartRepository, CartRepository>();

// Servicios de negocio.
builder.Services.AddScoped<ICartService, CartService>();

// Necesario para leer el Correlation ID desde clientes HTTP.
builder.Services.AddHttpContextAccessor();

// Handler para propagar X-Correlation-Id en llamadas salientes.
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

// Cliente HTTP hacia Products.API.
builder.Services
    .AddHttpClient<IProductsApiClient, ProductsApiClient>((serviceProvider, client) =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        var baseUrl = configuration["ExternalServices:ProductsApi:BaseUrl"]
            ?? "https://localhost:7001";

        client.BaseAddress = new Uri(baseUrl);
    })
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

// Exception handlers.
// específicos primero, global al final.
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Health Checks.
builder.Services.AddHealthChecks()
    .AddCheck<ApiStatusCheck>("api-status", tags: new[] { "live" })
    .AddCheck<SqliteHealthCheck>("sqlite-db", tags: new[] { "ready", "database" });

// Dashboard de Health Checks, según guía MiniApi.
builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(600);
    setup.AddHealthCheckEndpoint("Cart.API", "/health");
})
.AddInMemoryStorage();

var app = builder.Build();

// Inicialización de base al arrancar.
using (var scope = app.Services.CreateScope())
{
    var databaseInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    databaseInitializer.Initialize();
}

// Swagger disponible en Development.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Correlation ID antes de logging y handlers.
app.UseMiddleware<CorrelationIdMiddleware>();

// Logging automático de requests.
app.UseSerilogRequestLogging(options =>
{
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

// Health general.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Readiness: valida dependencias necesarias.
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

// Dashboard visual.
app.MapHealthChecksUI(setup =>
{
    setup.UIPath = "/health-ui";
});

app.Run();