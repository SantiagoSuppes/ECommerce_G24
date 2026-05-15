using ECommerce_G24.Cart.Api.Dtos;
using ECommerce_G24.Cart.Api.Services;
using ECommerce_G24.Cart.API.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_G24.Cart.API.Controller
{
    [ApiController]
    [Route("api/cart")]
    [Tags("Cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        // Inyectamos el servicio.
        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        /// <summary>
        /// Obtiene el carrito activo de un usuario.
        /// </summary>
        [HttpGet("{userId:guid}")]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartResponseDto>> GetCart(Guid userId)
        {
            var cart = await _cartService.GetCartAsync(userId);

            return Ok(cart);
        }

        /// <summary>
        /// Agrega un producto al carrito del usuario.
        /// </summary>
        [HttpPost("{userId:guid}/items")]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartResponseDto>> AddItem(
            Guid userId,
            [FromBody] AddCartItemRequestDto request)
        {
            var cart = await _cartService.AddItemAsync(userId, request);

            return Ok(cart);
        }

        /// <summary>
        /// Actualiza la cantidad de un producto dentro del carrito.
        /// </summary>
        [HttpPut("{userId:guid}/items/{productId:guid}")]
        [ProducesResponseType(typeof(CartResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CartResponseDto>> UpdateItem(
            Guid userId,
            Guid productId,
            [FromBody] UpdateCartItemRequestDto request)
        {
            var cart = await _cartService.UpdateItemAsync(userId, productId, request);

            return Ok(cart);
        }

        /// <summary>
        /// Quita un producto del carrito.
        /// </summary>
        [HttpDelete("{userId:guid}/items/{productId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteItem(Guid userId, Guid productId)
        {
            await _cartService.DeleteItemAsync(userId, productId);

            return NoContent();
        }

        /// <summary>
        /// Vacía el carrito del usuario.
        /// </summary>
        [HttpDelete("{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            await _cartService.ClearCartAsync(userId);

            return NoContent();
        }
    }
}
