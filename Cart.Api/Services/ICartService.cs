using ECommerce_G24.Cart.API.Dtos;

namespace ECommerce_G24.Cart.API.Services
{
    // Contrato del servicio de carrito.
    // El controller depende de esta interfaz, no directamente de CartService.
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(Guid userId);
        Task<CartResponseDto> AddItemAsync(Guid userId, AddCartItemRequestDto request);
        Task<CartResponseDto> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequestDto request);
        Task DeleteItemAsync(Guid userId, Guid productId);
        Task ClearCartAsync(Guid userId);
    }
}
