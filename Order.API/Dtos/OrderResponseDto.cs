namespace Orders.API.Dtos;

/// <summary>
/// DTO de respuesta de una orden.
/// </summary>
public class OrderResponseDto
{
    /// <summary>ID único de la orden.</summary>
    public Guid Id { get; set; }

    /// <summary>ID del usuario que realizó la orden.</summary>
    public string UsuarioId { get; set; } = string.Empty;

    /// <summary>Lista de items de la orden.</summary>
    public List<OrderItemResponseDto> Items { get; set; } = new();

    /// <summary>Total de la orden.</summary>
    public decimal Total { get; set; }

    /// <summary>Estado actual de la orden.</summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>Fecha de creación de la orden.</summary>
    public DateTime FechaCreacion { get; set; }
}

/// <summary>
/// DTO de respuesta de un item de la orden.
/// </summary>
public class OrderItemResponseDto
{
    /// <summary>ID del producto.</summary>
    public Guid ProductoId { get; set; }

    /// <summary>Cantidad del producto.</summary>
    public int Cantidad { get; set; }

    /// <summary>Precio unitario del producto.</summary>
    public decimal PrecioUnitario { get; set; }
}