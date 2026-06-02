namespace ECommerce_G24.Orders.API.Dtos;

public class CreateOrderRequestDto
{
    public string UsuarioId { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
}
