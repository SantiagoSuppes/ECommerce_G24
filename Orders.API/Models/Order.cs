namespace ECommerce_G24.Orders.API.Models;

public class Order
{
    public Guid Id { get; init; }
    public string UsuarioId { get; init; } = string.Empty;
    public List<OrderItem> Items { get; init; } = new();
    public decimal Total { get; init; }
    public string Estado { get; init; } = string.Empty;
    public DateTime FechaCreacion { get; init; }
}

public class OrderItem
{
    public Guid ProductoId { get; init; }
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
}
