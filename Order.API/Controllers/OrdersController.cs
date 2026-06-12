using Microsoft.AspNetCore.Mvc;
using Orders.API.Dtos;
using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers;

/// <summary>
/// Endpoints para crear, consultar y actualizar órdenes.
/// </summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[Tags("Orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(
        IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Lista todas las órdenes.
    /// Permite filtrar opcionalmente por usuario.
    /// </summary>
    /// <param name="usuarioId">
    /// Identificador opcional del usuario.
    /// </param>
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyCollection<OrderResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<IReadOnlyCollection<OrderResponseDto>>>
        GetAll([FromQuery] Guid? usuarioId)
    {
        var orders =
            await _orderService.GetAllAsync(
                usuarioId);

        return Ok(orders);
    }

    /// <summary>
    /// Obtiene el detalle de una orden.
    /// </summary>
    /// <param name="id">
    /// Identificador de la orden.
    /// </param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(OrderResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponseDto>>
        GetById(Guid id)
    {
        var order =
            await _orderService.GetByIdAsync(id);

        return Ok(order);
    }

    /// <summary>
    /// Crea una nueva orden.
    /// </summary>
    /// <remarks>
    /// Ejemplo:
    ///
    ///     POST /api/orders
    ///     {
    ///       "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
    ///       "items": [
    ///         {
    ///           "productoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    ///           "cantidad": 2
    ///         }
    ///       ]
    ///     }
    ///
    /// La API:
    ///
    /// - valida el usuario en Users.API;
    /// - valida cada producto en Products.API;
    /// - valida el stock;
    /// - captura el precio actual;
    /// - calcula el total;
    /// - crea la orden como Pendiente.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(
        typeof(OrderResponseDto),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponseDto>>
        Create(
            CreateOrderRequestDto request,
            CancellationToken cancellationToken)
    {
        var order =
            await _orderService.CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                id = order.Id
            },
            order);
    }

    /// <summary>
    /// Actualiza el estado de una orden.
    /// </summary>
    /// <param name="id">
    /// Identificador de la orden.
    /// </param>
    /// <remarks>
    /// Ejemplo:
    ///
    ///     PUT /api/orders/{id}/status
    ///     {
    ///       "estado": "Confirmada"
    ///     }
    ///
    /// Estados disponibles:
    ///
    /// - Pendiente
    /// - Confirmada
    /// - Enviada
    /// - Entregada
    /// - Cancelada
    /// </remarks>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(
        typeof(OrderStatusResponseDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ErrorResponseDto),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderStatusResponseDto>>
        UpdateStatus(
            Guid id,
            UpdateOrderStatusRequestDto request)
    {
        var response =
            await _orderService.UpdateStatusAsync(
                id,
                request);

        return Ok(response);
    }
}