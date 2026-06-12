using Orders.API.Dtos;
using Orders.API.DTOs;

namespace Orders.API.Services;

/// <summary>
/// Contrato de lógica de negocio de Orders.API.
/// </summary>
public interface IOrderService
{
    Task<IReadOnlyCollection<OrderResponseDto>> GetAllAsync(
        Guid? userId);

    Task<OrderResponseDto> GetByIdAsync(
        Guid orderId);

    Task<OrderResponseDto> CreateAsync(
        CreateOrderRequestDto request,
        CancellationToken cancellationToken = default);

    Task<OrderStatusResponseDto> UpdateStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequestDto request);
}