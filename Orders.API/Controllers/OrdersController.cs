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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAll([FromQuery] string? usuarioId)
    {
        var orders = await _orderService.GetAllAsync(usuarioId);
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponseDto>> GetById(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> Create([FromBody] CreateOrderRequestDto request)
    {
        var order = await _orderService.CreateAsync(request);
        return Created($"/api/orders/{order.Id}", order);
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<OrderResponseDto>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequestDto request)
    {
        var order = await _orderService.UpdateStatusAsync(id, request);
        return Ok(order);
    }
}
