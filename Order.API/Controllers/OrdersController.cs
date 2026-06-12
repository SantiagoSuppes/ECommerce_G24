using Orders.API.Services;
using Orders.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Orders.API.Controllers;

/// <summary>
/// Controlador para gestionar órdenes de compra.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Tags("Orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Obtiene todas las órdenes, opcionalmente filtradas por usuarioId.
    /// </summary>
    /// <param name="usuarioId">ID del usuario para filtrar (opcional).</param>
    /// <returns>Lista de órdenes.</returns>
    /// <response code="200">Lista obtenida exitosamente.</response>
    /// <response code="500">Error interno. Código: ORD-007.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAll([FromQuery] string? usuarioId)
    {
        var orders = await _orderService.GetAllAsync(usuarioId);
        return Ok(orders);
    }

    /// <summary>
    /// Obtiene una orden por su ID.
    /// </summary>
    /// <param name="id">ID de la orden.</param>
    /// <returns>Orden encontrada.</returns>
    /// <response code="200">Orden encontrada exitosamente.</response>
    /// <response code="404">Orden no encontrada. Código: ORD-001.</response>
    /// <response code="500">Error interno. Código: ORD-007.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponseDto>> GetById(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        return Ok(order);
    }

	/// <summary>
	/// Crea una nueva orden.
	/// </summary>
	/// <param name="request">Datos de la orden a crear.</param>
	/// <returns>Orden creada.</returns>
	/// <response code="201">Orden creada exitosamente.</response>
	/// <response code="400">Datos inválidos. Código: ORD-002.</response>
	/// <response code="404">Usuario o producto no encontrado. Código: ORD-003, ORD-004.</response>
	/// <response code="409">Conflicto de negocio. Código: ORD-006.</response>
	/// <response code="422">Stock insuficiente. Código: ORD-005.</response>
	/// <response code="500">Error interno. Código: ORD-007.</response>
	[HttpPost]
	[ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateOrderRequestDto request)
	{
		var order = await _orderService.CreateAsync(request);
		return Created($"/api/orders/{order.Id}", order);
	}

	/// <summary>
	/// Actualiza el estado de una orden existente.
	/// </summary>
	/// <param name="id">ID de la orden a actualizar.</param>
	/// <param name="request">Nuevo estado de la orden.</param>
	/// <returns>Orden actualizada.</returns>
	/// <response code="200">Estado actualizado exitosamente.</response>
	/// <response code="400">Estado inválido. Código: ORD-002.</response>
	/// <response code="404">Orden no encontrada. Código: ORD-001.</response>
	/// <response code="409">Transición de estado inválida. Código: ORD-006.</response>
	/// <response code="500">Error interno. Código: ORD-007.</response>
	[HttpPut("{id}/status")]
    [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequestDto request)
    {
        var order = await _orderService.UpdateStatusAsync(id, request);
        return Ok(order);
    }
}