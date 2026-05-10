namespace ECommerce_G24.src.Orders.API.Dtos;

public class OrderItemDto
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}
