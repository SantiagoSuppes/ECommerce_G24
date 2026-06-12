namespace Orders.API.DTOs;

/// <summary>
/// Respuesta luego de modificar el estado de una orden.
/// </summary>
public class OrderStatusResponseDto
{
    public Guid Id { get; set; }

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaActualizacion { get; set; }
}