using ECommerce_G24.Cart.Api.Dtos;
using ECommerce_G24.Cart.Api.Exceptions;
using ECommerce_G24.Cart.Api.Model;
using ECommerce_G24.Cart.API.Dtos;
using System.Net;

namespace ECommerce_G24.Cart.Api.Services
{
    public class CartService : ICartService
    {
        // Persistencia temporal en memoria.
        private static readonly List<Cart.Api.Model.Cart> _carts = new();

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CartService> _logger;

        public CartService(
            IHttpClientFactory httpClientFactory,
            ILogger<CartService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET /api/cart/{userId}
        public Task<CartResponseDto> GetCartAsync(Guid userId)
        {
            // Buscamos si existe un carrito activo para el usuario.
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

            // CRT-001 = Carrito no encontrado.
            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            return Task.FromResult(MapToResponse(cart));
        }

        // POST /api/cart/{userId}/items
        public async Task<CartResponseDto> AddItemAsync(Guid userId, AddCartItemRequestDto request)
        {
            // Validación de cantidad.
            if (request.Cantidad <= 0)
                throw new ValidationException("CRT-004", "Cantidad inválida.");

            // Validamos contra Products.API:
            var product = await GetProductOrThrowAsync(request.ProductoId);

            // Buscamos el carrito del usuario.
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

            // Si no existe carrito, se crea.
            var carritoEsNuevo = false;

            if (cart == null)
            {
                cart = new Cart.Api.Model.Cart
                {
                    UsuarioId = userId,
                    FechaActualizacion = DateTime.UtcNow
                };

                _carts.Add(cart);
                carritoEsNuevo = true;
            }

            // Buscamos si el producto ya estaba en el carrito.
            var item = cart.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);

            // Si ya estaba, se suma la cantidad.
            // Si no estaba, la cantidad final es la del request.
            var cantidadFinal = item == null
                ? request.Cantidad
                : item.Cantidad + request.Cantidad;

            // Validamos stock.
            if (cantidadFinal > product.Stock)
            {
                throw new BusinessRuleException(
                    "CRT-003",
                    $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {cantidadFinal}.");
            }

            // Si no existía el item, se agrega.
            if (item == null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductoId = request.ProductoId,
                    Cantidad = request.Cantidad
                });
            }
            else
            {
                // Si ya existía, se actualiza la cantidad acumulada.
                item.Cantidad = cantidadFinal;
            }

            // Actualizamos la fecha del carrito.
            cart.FechaActualizacion = DateTime.UtcNow;

            // Log informativo.
            _logger.LogInformation(
                "Producto agregado al carrito. UserId: {UserId}, ProductId: {ProductId}, Cantidad: {Cantidad}, CarritoNuevo: {CarritoNuevo}",
                userId,
                request.ProductoId,
                request.Cantidad,
                carritoEsNuevo);

            return MapToResponse(cart);
        }

        // PUT /api/cart/{userId}/items/{productId}
        public async Task<CartResponseDto> UpdateItemAsync(
            Guid userId,
            Guid productId,
            UpdateCartItemRequestDto request)
        {
            // Validamos cantidad.
            if (request.Cantidad <= 0)
                throw new ValidationException("CRT-004", "Cantidad inválida.");

            // Validamos que el producto exista en Products.API.
            var product = await GetProductOrThrowAsync(productId);

            // Validamos stock.
            if (request.Cantidad > product.Stock)
            {
                throw new BusinessRuleException(
                    "CRT-003",
                    $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {request.Cantidad}.");
            }

            // Buscamos carrito.
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            // Buscamos item dentro del carrito.
            var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId);

            // En este caso usamos CRT-002 porque el producto no está disponible
            // para esta operación de carrito.
            if (item == null)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            // Actualizamos cantidad.
            item.Cantidad = request.Cantidad;

            // Actualizamos fecha.
            cart.FechaActualizacion = DateTime.UtcNow;

            _logger.LogInformation(
                "Cantidad de item actualizada. UserId: {UserId}, ProductId: {ProductId}, Cantidad: {Cantidad}",
                userId,
                productId,
                request.Cantidad);

            return MapToResponse(cart);
        }

        // DELETE /api/cart/{userId}/items/{productId}
        public Task DeleteItemAsync(Guid userId, Guid productId)
        {
            // Buscamos carrito.
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            // Buscamos producto dentro del carrito.
            var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId);

            if (item == null)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            // Quitamos el item.
            cart.Items.Remove(item);

            // Actualizamos fecha.
            cart.FechaActualizacion = DateTime.UtcNow;

            _logger.LogInformation(
                "Producto eliminado del carrito. UserId: {UserId}, ProductId: {ProductId}",
                userId,
                productId);

            return Task.CompletedTask;
        }

        // DELETE /api/cart/{userId}
        public Task ClearCartAsync(Guid userId)
        {
            // Buscamos carrito.
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            // Eliminamos el carrito completo.
            _carts.Remove(cart);

            _logger.LogInformation(
                "Carrito vaciado. UserId: {UserId}",
                userId);

            return Task.CompletedTask;
        }

        // Método para consultar Products.API.
        private async Task<ProductResponseDto> GetProductOrThrowAsync(Guid productId)
        {
            // Creamos el cliente HTTP configurado en Program.cs.
            var client = _httpClientFactory.CreateClient("ProductsApi");

            // Llamamos a Products.API.
            var response = await client.GetAsync($"/api/products/{productId}");

            // Si Products.API devuelve 404, para Cart.API corresponde CRT-002.
            if (response.StatusCode == HttpStatusCode.NotFound)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            // Si Products.API devuelve otro error, lo tratamos como error interno.
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Error consultando Products.API. ProductId: {ProductId}. StatusCode: {StatusCode}",
                    productId,
                    response.StatusCode);

                throw new Exception("Error interno al consultar Products.API.");
            }

            // Deserializamos la respuesta JSON de Products.API.
            var product = await response.Content.ReadFromJsonAsync<ProductResponseDto>();

            // Si por algún motivo la respuesta vino vacía o mal formada,
            // lanzamos error inesperado.
            if (product == null)
                throw new Exception("Products.API devolvió una respuesta inválida.");

            return product;
        }

        // Mapeo de entidad interna a DTO de respuesta.
        private static CartResponseDto MapToResponse(Cart.Api.Model.Cart cart)
        {
            return new CartResponseDto
            {
                UsuarioId = cart.UsuarioId,
                FechaActualizacion = cart.FechaActualizacion,
                Items = cart.Items.Select(i => new CartItemResponseDto
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad
                }).ToList()
            };
        }
    }
}
