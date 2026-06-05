using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ECommerce_G24.Notifications.API.Controllers;

/// <summary>
/// Controlador para Health Check endpoints.
/// Verifica el estado y disponibilidad del servicio.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    /// <summary>
    /// Inicializa una nueva instancia del controlador Health.
    /// </summary>
    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Verifica el estado general de la API.
    /// </summary>
    /// <remarks>
    /// Valida que la API esté operativa. No realiza chequeos de dependencias específicas.
    /// </remarks>
    /// <response code="200">API está operativa.</response>
    [HttpGet("/health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetHealth()
    {
        _logger.LogInformation("Health check - General status requested");

        return Ok(new
        {
            status = HealthStatus.Healthy.ToString(),
            timestamp = DateTime.UtcNow,
            service = "Notifications.API",
            version = "1.0.0"
        });
    }

    ///<summary>
    /// Verifica si la API está viva (liveness probe).
    /// </summary>
    /// <remarks>
    /// Valida que el proceso esté en ejecución. No realiza chequeos de dependencias.
    /// </remarks>
    /// <response code="200">API está viva.</response>
    [HttpGet("/health/live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetHealthLive()
    {
        _logger.LogInformation("Health check - Liveness probe requested");

        return Ok(new
        {
            status = HealthStatus.Healthy.ToString(),
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Verifica si la API está lista para recibir tráfico (readiness probe).
    /// </summary>
    /// <remarks>
    /// Valida dependencias internas. Si alguna falla, devuelve 503 Service Unavailable.
    /// </remarks>
    /// <response code="200">API lista para recibir solicitudes.</response>
    /// <response code="503">API no está lista (dependencias no disponibles).</response>
    [HttpGet("/health/ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public ActionResult<object> GetHealthReady()
    {
        _logger.LogInformation("Health check - Readiness probe requested");

        // Aquí se pueden agregar validaciones de dependencias (BD, servicios externos, etc.)
        var isReady = true; // TODO: Validar dependencias reales

        if (!isReady)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = HealthStatus.Unhealthy.ToString(),
                timestamp = DateTime.UtcNow,
                reason = "Service dependencies not available"
            });
        }

        return Ok(new
        {
            status = HealthStatus.Healthy.ToString(),
            timestamp = DateTime.UtcNow
        });
    }
}
