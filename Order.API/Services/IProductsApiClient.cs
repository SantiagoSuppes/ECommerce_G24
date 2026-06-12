using Orders.API.Dtos;

namespace Orders.API.Clients;

public interface IProductsApiClient
{
    Task<ProductResponseDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default);
}