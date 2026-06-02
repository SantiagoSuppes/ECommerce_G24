using ECommerce_G24.Orders.API.Dtos;

namespace ECommerce_G24.Orders.API.Services;

public interface IOrderService
{
    Task<IEnumerable<OrderResponseDto>> GetAllAsync(string? usuarioId);
    Task<OrderResponseDto> GetByIdAsync(Guid id);
    Task<OrderResponseDto> CreateAsync(CreateOrderRequestDto request);
    Task<OrderResponseDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequestDto request);
}
