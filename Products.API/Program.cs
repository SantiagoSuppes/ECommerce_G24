using ECommerce_G24.Products.API.ExceptionHandlers;
using ECommerce_G24.Products.API.Middleware;
using ECommerce_G24.Products.API.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog.
// Se escribe en consola y en archivo JSON estructurado.
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithProperty("Servicio", "Products.API")
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/products-api-.json",
            rollingInterval: RollingInterval.Day,
            formatter: new Serilog.Formatting.Json.JsonFormatter());
});

// Se agregan controllers.
builder.Services.AddControllers();

// Se agregan servicios para Swagger.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Products API",
        Version = "v1",
        Description = "Microservicio para administrar productos del e-commerce."
    });
});

// Se registra la capa de servicios.
builder.Services.AddScoped<IProductService, ProductService>();

// Se registran los exception handlers.
// El orden importa: primero los específicos y último el global.
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Se agrega ProblemDetails porque forma parte del patrón recomendado con IExceptionHandler.
builder.Services.AddProblemDetails();

// Se agregan health checks.
builder.Services.AddHealthChecks();

var app = builder.Build();

// Swagger disponible en /swagger.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware para manejar X-Correlation-Id.
app.UseMiddleware<CorrelationIdMiddleware>();

// Logging automático de inicio y fin de cada request con duración.
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
});

// Manejo global de errores con IExceptionHandler.
// Esto cumple con el TP porque no se usa middleware personalizado para exceptions.
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.MapControllers();

// Health check general.
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

// Health check de readiness.
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

// Health check de liveness.
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