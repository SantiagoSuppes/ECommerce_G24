using Users.API.ExceptionHandlers;
using Users.API.Middleware;
using Users.API.Services;
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
        .Enrich.WithProperty("Servicio", "Users.API")
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/users-api-.json",
            rollingInterval: RollingInterval.Day,
            formatter: new Serilog.Formatting.Json.JsonFormatter());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Users API",
        Version = "v1",
        Description = "Microservicio para administrar usuarios del e-commerce."
    });
});
builder.Services.AddScoped<IUserService, UserService>();

// Register exception handlers
builder.Services.AddExceptionHandler<DuplicateEmailExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<InvalidCredentialsExceptionHandler>();
builder.Services.AddExceptionHandler<UserBlockedExceptionHandler>();
builder.Services.AddExceptionHandler<UserFraudBlockedExceptionHandler>();
builder.Services.AddExceptionHandler<InternalServerExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddProblemDetails();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CorrelationId middleware
app.UseMiddleware<CorrelationIdMiddleware>();

// Serilog request logging (registra duración)
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} respondió {StatusCode} en {Elapsed:0.0000} ms";
});

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapControllers();

// Health checks
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