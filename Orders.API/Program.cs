using ECommerce_G24.Orders.API.ExceptionHandlers;
using ECommerce_G24.Orders.API.Middleware;
using ECommerce_G24.Orders.API.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Serilog.
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Servicio", "Orders.API")
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/orders-api-.json",
            rollingInterval: RollingInterval.Day,
            formatter: new Serilog.Formatting.Json.JsonFormatter());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger con soporte para XML comments.
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

builder.Services.AddScoped<IOrderService, OrderService>();

// Exception handlers.
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BussinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<InternalServerExceptionHandler>();
builder.Services.AddProblemDetails();

// Health checks.
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CorrelationId middleware.
app.UseMiddleware<CorrelationIdMiddleware>();

// Serilog request logging (registra duración automáticamente).
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
});

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();

// Health checks.
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