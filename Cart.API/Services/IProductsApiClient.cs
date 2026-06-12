using Cart.API.Cart.API.Dtos;

namespace Cart.API.Cart.API.Services
{
    // Contrato para consultar Products.API.
    // Cart.API lo usa para validar producto existente y stock.
    
    public interface IProductsApiClient
    {
        Task<ProductResponseDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
    }
}
