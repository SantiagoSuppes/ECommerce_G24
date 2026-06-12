using System.Net;
using Orders.API.Dtos;

namespace Orders.API.Clients;

public class ProductsApiClient : IProductsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsApiClient> _logger;

    public ProductsApiClient(HttpClient httpClient, ILogger<ProductsApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductResponseDto?> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/products/{productId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var product = await response.Content.ReadFromJsonAsync<ProductResponseDto>(cancellationToken: cancellationToken);

        _logger.LogInformation("Producto consultado desde Products.API. ProductId: {ProductId}", productId);
        return product;
    }
}