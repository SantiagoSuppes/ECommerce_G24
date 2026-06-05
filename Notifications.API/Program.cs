using Serilog;
using Microsoft.OpenApi;
using ECommerce_G24.Notifications.API.ExceptionHandlers;
using ECommerce_G24.Notifications.API.Services;
using ECommerce_G24.Notifications.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseSerilog((context, services, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.WithProperty("Application", "Notifications.API")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
);

// Agregar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configurar Swagger con XML Comments
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Notifications API",
        Version = "v1",
        Description = "API REST para la gestión de notificaciones en el e-commerce",
        Contact = new OpenApiContact
        {
            Name = "ECommerce G24 Team",
            Email = "support@ecommerce.com"
        }
    });

    // Agregar soporte para XML comments
    var xmlFile = Path.Combine(AppContext.BaseDirectory, "ECommerce_G24.xml");
    if (File.Exists(xmlFile))
    {
        options.IncludeXmlComments(xmlFile);
    }
});

// Registrar servicios
builder.Services.AddScoped<INotificationService, NotificationService>();

// Configurar Exception Handlers
builder.Services.AddExceptionHandler<UserNotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<InternalServerExceptionHandler>();
builder.Services.AddProblemDetails();

// Configurar CORS (opcional pero recomendado)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Usar el middleware de Exception Handler
app.UseExceptionHandler();

// Usar Correlation ID middleware
app.UseCorrelationId();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Notifications API v1");
        options.RoutePrefix = "swagger"; 
    });
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.MapControllers();

// Logging de inicio exitoso
app.Logger.LogInformation("Notifications API iniciada correctamente en ambiente: {Environment}", 
    app.Environment.EnvironmentName);

app.Run();
