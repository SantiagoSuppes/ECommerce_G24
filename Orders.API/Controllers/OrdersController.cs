using Microsoft.AspNetCore.Mvc;
using ECommerce_G24.Orders.API.Dtos;
using ECommerce_G24.Orders.API.Services;

namespace ECommerce_G24.Orders.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    /// <param name="usuarioId">El ID del usuario para filtrar las órdenes (opcional).</param>
    /// <returns>Una lista de órdenes.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAll([FromQuery] string? usuarioId)
    {
        var orders = await _orderService.GetAllAsync(usuarioId);
        return Ok(orders);
    }


    /// <summary>
    /// Obtiene una orden por su ID.
    /// </summary>
    /// <param name="id">El ID de la orden.</param>
    /// <returns>La orden correspondiente al ID proporcionado.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponseDto>> GetById(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        return Ok(order);
    }

    /// <summary>
    /// Crea una nueva orden con los datos proporcionados en el cuerpo de la solicitud.
    /// </summary>
    /// <param name="request">Los datos de la orden a crear.</param>
    /// <returns>La orden creada.</returns>
    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateOrderRequestDto request)
    {
        var order = await _orderService.CreateAsync(request);
        return Created($"/api/orders/{order.Id}", order);
    }


    /// <summary>
    /// Actualiza el estado de una orden existente identificada por su ID, utilizando los datos proporcionados en el cuerpo de la solicitud.
    /// </summary>
    /// <param name="id">El ID de la orden a actualizar.</param>
    /// <param name="request">Los datos para actualizar el estado de la orden.</param>
    /// <returns>La orden actualizada.</returns>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<OrderResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequestDto request)
    {
        var order = await _orderService.UpdateStatusAsync(id, request);
        return Ok(order);
    }
}
