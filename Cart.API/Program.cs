using ECommerce_G24.Cart.API.ExceptionHandlers;
using ECommerce_G24.Cart.API.Middleware;
using ECommerce_G24.Cart.API.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog.
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .Enrich.FromLogContext()

        // Agregamos el nombre del microservicio a todos los logs.
        .Enrich.WithProperty("Servicio", "Cart.API")

        // Log en consola.
        .WriteTo.Console()

        // Log en archivo JSON.
        .WriteTo.File(
            path: "logs/cart-api-.json",
            rollingInterval: RollingInterval.Day,
            formatter: new Serilog.Formatting.Json.JsonFormatter());
});

// Registramos controllers.
builder.Services.AddControllers();

// Habilitamos Swagger.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cart API",
        Version = "v1",
        Description = "API para gestión del carrito de compras del e-commerce."
    });

    // Esto permite que Swagger lea los comentarios XML de controllers, models y DTOs.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Necesario para leer el HttpContext desde CorrelationIdDelegatingHandler.
builder.Services.AddHttpContextAccessor();

// Registramos el handler que propaga X-Correlation-Id a llamadas HTTP salientes.
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

// Configuramos HttpClient para llamar a Products.API.
builder.Services.AddHttpClient("ProductsApi", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Services:ProductsApi"]
        ?? "https://localhost:7001");
})
.AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

// Registramos el servicio de carrito.
builder.Services.AddScoped<ICartService, CartService>();

// Registramos los handlers de excepciones.
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Necesario para ProblemDetails
builder.Services.AddProblemDetails();

// Health Checks 
builder.Services.AddHealthChecks();

var app = builder.Build();

// Middleware de Correlation ID.
app.UseMiddleware<CorrelationIdMiddleware>();

// Log automático de inicio/fin de requests con duración.
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
});

// Activa el manejo global de errores con IExceptionHandler.
app.UseExceptionHandler();

// Swagger en ambiente de desarrollo.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Mapeamos controllers.
app.MapControllers();

// Health Check general.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString()
        });
    }
});

// Health Check de readiness.
// Indica si el servicio está listo para recibir tráfico.
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString()
        });
    }
});

// Health Check de liveness.
// Indica si el servicio está vivo.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString()
        });
    }
});

app.Run();