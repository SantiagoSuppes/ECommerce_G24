using Orders.API.Clients;
using Orders.API.Dtos;
using Orders.API.Exceptions;
using Orders.API.Models;
using Orders.API.Repositories;

namespace Orders.API.Services;

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

    public async Task<IEnumerable<OrderResponseDto>> GetAllAsync(string? usuarioId)
    {
        try
        {
            _logger.LogInformation("Obteniendo ordenes. UsuarioId: {UsuarioId}", usuarioId ?? "todos");
            var orders = await _repository.GetAllAsync(usuarioId);
            return orders.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al obtener ordenes");
            throw new InternalServerException("ORD-007", "Error interno al procesar la orden.");
        }
    }

    public async Task<OrderResponseDto> GetByIdAsync(Guid id)
    {
        try
        {
            var order = await _repository.GetByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Orden no encontrada. ID: {Id}", id);
                throw new NotFoundException("ORD-001", $"Orden no encontrada. ID: {id}");
            }

            return MapToDto(order);
        }
        catch (NotFoundException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al obtener orden. ID: {Id}", id);
            throw new InternalServerException("ORD-007", "Error interno al procesar la orden.");
        }
    }

    public async Task<OrderResponseDto> CreateAsync(CreateOrderRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UsuarioId))
            {
                _logger.LogWarning("Validacion fallida: UsuarioId vacio");
                throw new ValidationException("ORD-002", "El UsuarioId es requerido");
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                _logger.LogWarning("Validacion fallida: orden sin items. UsuarioId: {UsuarioId}", request.UsuarioId);
                throw new ValidationException("ORD-002", "La orden debe contener al menos un producto");
            }

            // Validar usuario en Users.API
            var userExists = await _usersApiClient.UserExistsAsync(Guid.Parse(request.UsuarioId));
            if (!userExists)
            {
                _logger.LogWarning("Usuario no encontrado. UsuarioId: {UsuarioId}", request.UsuarioId);
                throw new NotFoundException("ORD-003", "Usuario no encontrado al crear la orden.");
            }

            // Validar productos y stock en Products.API
            var orderItems = new List<OrderItem>();
            decimal total = 0;

            foreach (var item in request.Items)
            {
                var product = await _productsApiClient.GetProductByIdAsync(item.ProductoId);

                if (product == null)
                {
                    _logger.LogWarning("Producto no encontrado. ProductoId: {ProductoId}", item.ProductoId);
                    throw new NotFoundException("ORD-004", "Producto no encontrado al crear la orden.");
                }

                if (item.Cantidad > product.Stock)
                {
                    _logger.LogWarning("Stock insuficiente. ProductoId: {ProductoId}, Stock: {Stock}, Solicitado: {Cantidad}",
                        item.ProductoId, product.Stock, item.Cantidad);
                    throw new UnprocessableEntityException("ORD-005",
                        $"Stock insuficiente para '{product.Nombre}'. Disponible: {product.Stock}, solicitado: {item.Cantidad}.");
                }

                orderItems.Add(new OrderItem
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = product.Precio
                });

                total += item.Cantidad * product.Precio;
            }

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UsuarioId = request.UsuarioId,
                Items = orderItems,
                Total = total,
                Estado = "Pendiente",
                FechaCreacion = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(order, orderItems);

            _logger.LogInformation("Orden creada. ID: {Id}, UsuarioId: {UsuarioId}, Total: {Total}",
                order.Id, order.UsuarioId, total);

            return MapToDto(created);
        }
        catch (NotFoundException) { throw; }
        catch (ValidationException) { throw; }
        catch (UnprocessableEntityException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al crear orden");
            throw new InternalServerException("ORD-007", "Error interno al procesar la orden.");
        }
    }

    public async Task<OrderResponseDto> UpdateStatusAsync(Guid id, UpdateOrderStatusRequestDto request)
    {
        try
        {
            var order = await _repository.GetByIdAsync(id);

            if (order == null)
            {
                _logger.LogWarning("Orden no encontrada para actualizar. ID: {Id}", id);
                throw new NotFoundException("ORD-001", $"Orden no encontrada. ID: {id}");
            }

            var transicionesValidas = new Dictionary<string, string[]>
            {
                { "Pendiente",  new[] { "Confirmada", "Cancelada" } },
                { "Confirmada", new[] { "Enviada", "Cancelada" } },
                { "Enviada",    new[] { "Entregada" } },
                { "Entregada",  Array.Empty<string>() },
                { "Cancelada",  Array.Empty<string>() }
            };

            if (!transicionesValidas.TryGetValue(order.Estado, out var permitidos) ||
                !permitidos.Contains(request.Estado))
            {
                _logger.LogWarning("Transicion invalida: {EstadoActual} -> {EstadoNuevo}. ID: {Id}",
                    order.Estado, request.Estado, id);
                throw new BusinessRuleException("ORD-006",
                    $"Una orden en estado '{order.Estado}' no puede cambiar a '{request.Estado}'.");
            }

            await _repository.UpdateStatusAsync(id, request.Estado);

            _logger.LogInformation("Estado actualizado. ID: {Id}, Estado: {Estado}", id, request.Estado);

            return await GetByIdAsync(id);
        }
        catch (NotFoundException) { throw; }
        catch (BusinessRuleException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno al actualizar estado. ID: {Id}", id);
            throw new InternalServerException("ORD-007", "Error interno al procesar la orden.");
        }
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