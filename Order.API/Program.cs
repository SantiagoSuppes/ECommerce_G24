using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Orders.API.Clients;
using Orders.API.Database;
using Orders.API.ExceptionHandlers;
using Orders.API.HealthChecks;
using Orders.API.Middleware;
using Orders.API.Services;
using Orders.API.Repositories;
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
        Title = "Orders API",
        Version = "v1",
        Description = "Microservicio para gestión de órdenes del e-commerce."
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// HTTP Context Accessor.
builder.Services.AddHttpContextAccessor();

// Handler para propagar X-Correlation-Id en llamadas salientes.
builder.Services.AddTransient<CorrelationIdDelegatingHandler>();

// Cliente HTTP hacia Users.API.
builder.Services
    .AddHttpClient<IUsersApiClient, UsersApiClient>((serviceProvider, client) =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var baseUrl = configuration["ExternalServices:UsersApi:BaseUrl"] ?? "http://localhost:5135";
        client.BaseAddress = new Uri(baseUrl);
    })
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

// Cliente HTTP hacia Products.API.
builder.Services
    .AddHttpClient<IProductsApiClient, ProductsApiClient>((serviceProvider, client) =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var baseUrl = configuration["ExternalServices:ProductsApi:BaseUrl"] ?? "http://localhost:5001";
        client.BaseAddress = new Uri(baseUrl);
    })
    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>();

// Exception handlers.
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<UnprocessableEntityExceptionHandler>();
builder.Services.AddExceptionHandler<InternalServerExceptionHandler>();
builder.Services.AddProblemDetails();

// Health checks.
builder.Services.AddHealthChecks()
    .AddCheck<ApiStatusCheck>("api-status", tags: new[] { "live" })
    .AddCheck<SqliteHealthCheck>("sqlite-db", tags: new[] { "ready", "database" });

// Inicialización de la base de datos.
builder.Services.AddSingleton<DatabaseInitializer>();

var app = builder.Build();

// Inicializar la base de datos al iniciar la aplicación.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    db.Initialize();
}

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
    Predicate = check => check.Tags.Contains("ready"),
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
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = async (context, report) =>
    {
        await context.Response.WriteAsJsonAsync(new
        {
            status = report.Status.ToString()
        });
    }
});

app.Run();