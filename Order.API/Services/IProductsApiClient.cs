using Orders.API.Dtos;

namespace Orders.API.Services;

/// <summary>
/// Contrato para consultar Products.API.
/// </summary>
public interface IProductsApiClient
{
    /// <summary>
    /// Obtiene un producto por ID.
    /// </summary>
    Task<ProductResponseDto?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default);
}