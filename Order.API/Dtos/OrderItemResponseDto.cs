namespace Orders.API.DTOs;

/// <summary>
/// Item devuelto en las respuestas de Orders.API.
/// </summary>
public class OrderItemResponseDto
{
    public Guid ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }
}