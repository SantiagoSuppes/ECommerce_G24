using ECommerce_G24.Cart.API.Dtos;
using System.Net;

namespace ECommerce_G24.Cart.API.Services
{
    // Cliente HTTP para Products.API.    
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

        public async Task<ProductResponseDto?> GetProductByIdAsync(
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            // Llama al endpoint definido por Products.API.
            var response = await _httpClient.GetAsync($"/api/products/{productId}", cancellationToken);

            // Si Products.API devuelve 404, Cart.API lo traduce luego a CRT-002.
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            // Cualquier otro error inesperado se deja escalar como excepción.
            response.EnsureSuccessStatusCode();

            var product = await response.Content.ReadFromJsonAsync<ProductResponseDto>(
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Producto consultado desde Products.API. ProductId: {ProductId}",
                productId);

            return product;
        }
    }
}
