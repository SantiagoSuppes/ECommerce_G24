using Orders.API.Dtos;
using Orders.API.DTOs;
using Orders.API.Exceptions;
using Orders.API.Models;
using Orders.API.Repositories;

namespace Orders.API.Services;

/// <summary>
/// Servicio principal de Orders.API.
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly IUsersApiClient _usersApiClient;
    private readonly IProductsApiClient _productsApiClient;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository repository,
        IUsersApiClient usersApiClient,
        IProductsApiClient productsApiClient,
        ILogger<OrderService> logger)
    {
        _repository = repository;
        _usersApiClient = usersApiClient;
        _productsApiClient = productsApiClient;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<OrderResponseDto>>
        GetAllAsync(Guid? userId)
    {
        var orders =
            await _repository.GetAllAsync(userId);

        return orders
            .Select(MapToResponse)
            .ToList();
    }

    public async Task<OrderResponseDto> GetByIdAsync(
        Guid orderId)
    {
        var order =
            await _repository.GetByIdAsync(orderId);

        if (order is null)
        {
            throw new NotFoundException(
                OrderErrorCodes.OrderNotFound,
                "Orden no encontrada.");
        }

        return MapToResponse(order);
    }

    public async Task<OrderResponseDto> CreateAsync(
        CreateOrderRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateRequest(request);

        // Valida que el usuario exista en Users.API.
        var userExists =
            await _usersApiClient.UserExistsAsync(
                request.UsuarioId,
                cancellationToken);

        if (!userExists)
        {
            throw new NotFoundException(
                OrderErrorCodes.UserNotFound,
                "Usuario no encontrado al crear la orden.");
        }

        /*
         * Si un mismo ProductoId aparece más de una vez,
         * se suman sus cantidades.
         *
         * Esto evita guardar dos filas del mismo producto
         * y permite validar correctamente el stock total solicitado.
         */
        var groupedItems =
            request.Items
                .GroupBy(item => item.ProductoId)
                .Select(group => new
                {
                    ProductoId = group.Key,
                    Cantidad =
                        group.Sum(item => item.Cantidad)
                })
                .ToList();

        var orderItems =
            new List<OrderItem>();

        foreach (var requestedItem in groupedItems)
        {
            var product =
                await _productsApiClient.GetByIdAsync(
                    requestedItem.ProductoId,
                    cancellationToken);

            if (product is null)
            {
                throw new NotFoundException(
                    OrderErrorCodes.ProductNotFound,
                    "Producto no encontrado al crear la orden.");
            }

            if (requestedItem.Cantidad > product.Stock)
            {
                throw new BusinessRuleException(
                    OrderErrorCodes.InsufficientStock,

                    $"Stock insuficiente para '{product.Nombre}'. " +
                    $"Disponible: {product.Stock}, " +
                    $"solicitado: {requestedItem.Cantidad}.",

                    StatusCodes.Status422UnprocessableEntity);
            }

            orderItems.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductoId = product.Id,
                Cantidad = requestedItem.Cantidad,
                PrecioUnitario = product.Precio
            });
        }

        // Calcula el total automáticamente.
        var total =
            orderItems.Sum(
                item =>
                    item.Cantidad *
                    item.PrecioUnitario);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Items = orderItems,
            Total = total,
            Estado = OrderStatuses.Pending,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = null
        };

        var createdOrder =
            await _repository.CreateAsync(order);

        _logger.LogInformation(
            "Orden creada. OrdenId: {OrdenId}, UsuarioId: {UsuarioId}, Total: {Total}",
            createdOrder.Id,
            createdOrder.UsuarioId,
            createdOrder.Total);

        return MapToResponse(createdOrder);
    }

    public async Task<OrderStatusResponseDto> UpdateStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Estado))
        {
            throw new ValidationException(
                OrderErrorCodes.InvalidOrderData,
                "El estado de la orden es obligatorio.");
        }

        if (!OrderStatuses.IsValid(request.Estado))
        {
            throw new ValidationException(
                OrderErrorCodes.InvalidOrderData,

                "El estado debe ser Pendiente, Confirmada, " +
                "Enviada, Entregada o Cancelada.");
        }

        var order =
            await _repository.GetByIdAsync(orderId);

        if (order is null)
        {
            throw new NotFoundException(
                OrderErrorCodes.OrderNotFound,
                "Orden no encontrada.");
        }

        var newStatus =
            OrderStatuses.Normalize(
                request.Estado);

        if (!OrderStatuses.CanTransition(
                order.Estado,
                newStatus))
        {
            throw new BusinessRuleException(
                OrderErrorCodes.InvalidStatusTransition,

                $"Una orden en estado '{order.Estado}' " +
                $"no puede pasar a '{newStatus}'.",

                StatusCodes.Status409Conflict);
        }

        var updatedAt =
            DateTime.UtcNow;

        await _repository.UpdateStatusAsync(
            orderId,
            newStatus,
            updatedAt);

        _logger.LogInformation(
            "Estado de orden actualizado. OrdenId: {OrdenId}, EstadoAnterior: {EstadoAnterior}, EstadoNuevo: {EstadoNuevo}",
            orderId,
            order.Estado,
            newStatus);

        return new OrderStatusResponseDto
        {
            Id = orderId,
            Estado = newStatus,
            FechaActualizacion = updatedAt
        };
    }

    private static void ValidateCreateRequest(
        CreateOrderRequestDto request)
    {
        if (request.UsuarioId == Guid.Empty)
        {
            throw new ValidationException(
                OrderErrorCodes.InvalidOrderData,
                "El usuario es obligatorio.");
        }

        if (request.Items is null ||
            request.Items.Count == 0)
        {
            throw new ValidationException(
                OrderErrorCodes.InvalidOrderData,
                "La orden debe contener al menos un item.");
        }

        foreach (var item in request.Items)
        {
            if (item.ProductoId == Guid.Empty)
            {
                throw new ValidationException(
                    OrderErrorCodes.InvalidOrderData,
                    "El producto es obligatorio.");
            }

            if (item.Cantidad <= 0)
            {
                throw new ValidationException(
                    OrderErrorCodes.InvalidOrderData,
                    "La cantidad debe ser mayor a cero.");
            }
        }
    }

    private static OrderResponseDto MapToResponse(
        Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            UsuarioId = order.UsuarioId,

            Items = order.Items
                .Select(item =>
                    new OrderItemResponseDto
                    {
                        ProductoId =
                            item.ProductoId,

                        Cantidad =
                            item.Cantidad,

                        PrecioUnitario =
                            item.PrecioUnitario
                    })
                .ToList(),

            Total = order.Total,
            Estado = order.Estado,
            FechaCreacion = order.FechaCreacion
        };
    }
}