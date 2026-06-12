using Orders.API.Dtos;
using System.Net;
using System.Net.Http.Json;

namespace Orders.API.Services;

/// <summary>
/// Cliente HTTP para consultar Products.API.
/// </summary>
public class ProductsApiClient : IProductsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsApiClient> _logger;

    public ProductsApiClient(
        HttpClient httpClient,
        ILogger<ProductsApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ProductResponseDto?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var requestUrl =
            $"/api/products/{productId}";

        _logger.LogInformation(
            "Consultando producto en {BaseAddress}{RequestUrl}",
            _httpClient.BaseAddress,
            requestUrl);

        var response =
            await _httpClient.GetAsync(
                requestUrl,
                cancellationToken);

        
        if (response.StatusCode ==
            HttpStatusCode.NotFound)
        {
            return null;
        }

        
        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ProductResponseDto>(
                cancellationToken:
                    cancellationToken);
    }
}