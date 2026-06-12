using Orders.API.Models;

namespace Orders.API.Repositories;

/// <summary>
/// Contrato de persistencia de órdenes.
/// </summary>
public interface IOrderRepository
{
    Task<IReadOnlyCollection<Order>> GetAllAsync(
        Guid? userId);

    Task<Order?> GetByIdAsync(
        Guid orderId);

    Task<Order> CreateAsync(
        Order order);

    Task UpdateStatusAsync(
        Guid orderId,
        string status,
        DateTime updatedAt);
}