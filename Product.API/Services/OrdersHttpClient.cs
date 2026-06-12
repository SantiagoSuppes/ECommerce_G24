
using ECommerce_G24.Products.API.Dtos;

namespace ECommerce_G24.Products.API.Services
{
    /// <summary>
    /// Cliente HTTP encargado de consultar Orders.API.
    /// </summary>
    public class OrdersHttpClient : IOrdersClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrdersHttpClient> _logger;

        public OrdersHttpClient(
            HttpClient httpClient,
            ILogger<OrdersHttpClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> HasActiveOrdersAsync(
            Guid productId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Consultando Orders.API para verificar órdenes activas del producto {ProductId}",
                productId);


            using var response = await _httpClient.GetAsync(
                "api/orders",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody =
                    await response.Content.ReadAsStringAsync(
                        cancellationToken);

                _logger.LogError(
                    "Error al consultar Orders.API. ProductId: {ProductId}, StatusCode: {StatusCode}, Response: {Response}",
                    productId,
                    (int)response.StatusCode,
                    responseBody);

                /*
                 * Si no fue posible consultar Orders.API, no se debe
                 * continuar con la eliminación porque no puede comprobarse
                 * que el producto esté libre de órdenes activas.
                 *
                 * La excepción será convertida por el handler global
                 * en PRD-005.
                 */
                throw new HttpRequestException(
                    $"Orders.API respondió con estado {(int)response.StatusCode}.");
            }

            var orders =
                await response.Content.ReadFromJsonAsync<List<OrderResponseDto>>(
                    cancellationToken: cancellationToken);

            if (orders is null)
            {
                _logger.LogError(
                    "Orders.API devolvió una respuesta inválida al verificar el producto {ProductId}",
                    productId);

                throw new InvalidOperationException(
                    "Orders.API devolvió una respuesta inválida.");
            }

            /*
             * Las órdenes que activas son:
             *
             * - Pendiente
             * - Confirmada
             *
             * Además, la orden debe contener un item cuyo ProductoId
             * coincida con el producto que se intenta eliminar.
             */
            var hasActiveOrders = orders.Any(order =>
                IsActiveStatus(order.Estado) &&
                order.Items.Any(item => item.ProductoId == productId));

            _logger.LogInformation(
                "Verificación finalizada para producto {ProductId}. Tiene órdenes activas: {HasActiveOrders}",
                productId,
                hasActiveOrders);

            return hasActiveOrders;
        }

        private static bool IsActiveStatus(string estado)
        {
            return estado.Equals(
                       "Pendiente",
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   estado.Equals(
                       "Confirmada",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}

