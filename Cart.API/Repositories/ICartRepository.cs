
using CartModel = ECommerce_G24.Cart.API.Model.Cart;
namespace ECommerce_G24.Cart.API.Repositories
{
    // Contrato de persistencia para Cart.API.
    public interface ICartRepository
    {
        Task<CartModel?> GetByUserIdAsync(Guid userId);

        Task<bool> CartExistsAsync(Guid userId);

        Task<bool> ItemExistsAsync(Guid userId, Guid productId);

        Task<int?> GetItemQuantityAsync(Guid userId, Guid productId);

        Task UpsertItemAsync(Guid userId, Guid productId, int quantity, DateTime updatedAt);

        Task UpdateItemAsync(Guid userId, Guid productId, int quantity, DateTime updatedAt);

        Task RemoveItemAsync(Guid userId, Guid productId);

        Task ClearCartAsync(Guid userId);
    }
}
