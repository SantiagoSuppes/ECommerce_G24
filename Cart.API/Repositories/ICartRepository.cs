
using CartModel = Cart.API.Cart.API.Model.Cart;
namespace Cart.API.Cart.API.Repositories
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

        Task DeleteItemAsync(Guid userId, Guid productId);

        Task ClearCartAsync(Guid userId);
    }
}
