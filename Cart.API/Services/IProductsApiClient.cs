using ECommerce_G24.Cart.API.Dtos;

namespace ECommerce_G24.Cart.API.Services
{
    // Contrato para consultar Products.API.
    // Cart.API lo usa para validar producto existente y stock.
    
    public interface IProductsApiClient
    {
        Task<ProductResponseDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    }
}
