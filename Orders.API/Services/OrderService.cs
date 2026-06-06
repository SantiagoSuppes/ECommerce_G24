using ECommerce_G24.Orders.API.Dtos;
using ECommerce_G24.Orders.API.Exceptions;
using ECommerce_G24.Orders.API.Models;
using Microsoft.Extensions.Logging;

namespace ECommerce_G24.Orders.API.Services;

public class OrderService : IOrderService
{
    private readonly List<Order> _orders = new();
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllAsync(string? usuarioId)
    {
        _logger.LogInformation("Obteniendo ordenes. UsuarioId: {UsuarioId}", usuarioId ?? "todos");
        return await Task.FromResult(_orders
            .Where(o => usuarioId == null || o.UsuarioId == usuarioId)
            .Select(MapToDto)
            .ToList());
    }

    public async Task<OrderResponseDto> GetByIdAsync(Guid id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            _logger.LogWarning("Orden no encontrada. ID: {Id}", id);
            throw new NotFoundException($"Orden no encontrada. ID: {id}");
        }

        return await Task.FromResult(MapToDto(order));
    }

    public async Task<OrderResponseDto> CreateAsync(CreateOrderRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.UsuarioId))
        {
            _logger.LogWarning("Validacion fallida: UsuarioId vacio");
            throw new ValidationException("El UsuarioId es requerido");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            _logger.LogWarning("Validacion fallida: orden sin items. UsuarioId: {UsuarioId}", request.UsuarioId);
            throw new ValidationException("La orden debe contener al menos un producto");
        }

        var total = request.Items.Sum(i => i.Cantidad * i.PrecioUnitario);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Items = request.Items.Select(i => new OrderItem
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList(),
            Total = total,
            Estado = "Pendiente",
            FechaCreacion = DateTime.UtcNow
        };

        _orders.Add(order);
        _logger.LogInformation("Orden creada. ID: {Id}, UsuarioId: {UsuarioId}, Total: {Total}", order.Id, order.UsuarioId, order.Total);
        return await Task.FromResult(MapToDto(order));
    }

    public async Task<OrderResponseDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequestDto request)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null)
        {
            _logger.LogWarning("Orden no encontrada para actualizar. ID: {Id}", id);
            throw new NotFoundException($"Orden no encontrada. ID: {id}");
        }

        var validStates = new[] { "Pendiente", "Confirmada", "Enviada", "Entregada", "Cancelada" };
        if (!validStates.Contains(request.Estado))
        {
            _logger.LogWarning("Estado invalido: {Estado}. ID: {Id}", request.Estado, id);
            throw new ValidationException($"El estado '{request.Estado}' no es válido");
        }

        if (order.Estado == "Entregada" && request.Estado != "Entregada")
        {
            _logger.LogWarning("Intento de modificar orden entregada. ID: {Id}", id);
            throw new BussinessRuleException("No se puede modificar el estado de una orden 'Entregada'");
        }

        var updatedOrder = new Order
        {
            Id = order.Id,
            UsuarioId = order.UsuarioId,
            Items = order.Items,
            Total = order.Total,
            Estado = request.Estado,
            FechaCreacion = order.FechaCreacion
        };

        var index = _orders.IndexOf(order);
        _orders[index] = updatedOrder;

        _logger.LogInformation("Estado actualizado. ID: {Id}, Estado: {Estado}", updatedOrder.Id, updatedOrder.Estado);
        return await Task.FromResult(MapToDto(updatedOrder));
    }

    private static OrderResponseDto MapToDto(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UsuarioId = order.UsuarioId,
            Items = order.Items.Select(i => new OrderItemResponseDto
            {
                ProductoId = i.ProductoId,
                Cantidad = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario
            }).ToList(),
            Total = order.Total,
            Estado = order.Estado,
            FechaCreacion = order.FechaCreacion
        };
    }
}