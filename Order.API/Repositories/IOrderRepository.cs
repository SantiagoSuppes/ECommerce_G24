using Orders.API.Models;

namespace Orders.API.Repositories;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync(string? usuarioId);
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> CreateAsync(Order order, List<OrderItem> items);
    Task UpdateStatusAsync(Guid id, string estado);
}