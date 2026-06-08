using Cart.API.Cart.API.Dtos;
using Cart.API.Cart.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cart.API.Cart.API.Controller
{
    [ApiController]
    [Route("api/cart")]
    [Produces("application/json")]
    [Tags("Cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

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
            AddCartItemRequestDto request)
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
            UpdateCartItemRequestDto request)
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
