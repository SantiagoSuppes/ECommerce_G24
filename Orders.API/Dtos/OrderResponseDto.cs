namespace ECommerce_G24.Orders.API.Dtos;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public List<OrderItemResponseDto> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}

public class OrderItemResponseDto
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
