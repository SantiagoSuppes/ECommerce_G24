using ECommerce_G24.Cart.Api.Dtos;

namespace ECommerce_G24.Cart.Api.Services
{
    public interface ICartService
    {
        Task<CartResponseDto> GetCartAsync(Guid userId);
        Task<CartResponseDto> AddItemAsync(Guid userId, AddCartItemRequestDto request);
        Task<CartResponseDto> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequestDto request);
        Task DeleteItemAsync(Guid userId, Guid productId);
        Task ClearCartAsync(Guid userId);
    }
}
