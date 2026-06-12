using Cart.API.Cart.API.Dtos;
using Cart.API.Cart.API.Exceptions;
using Cart.API.Cart.API.Model;
using Cart.API.Cart.API.Repositories;
using Cart.API.Cart.API.Services;
using System.Net;

namespace Cart.API.Cart.API.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _repository;
        private readonly IProductsApiClient _productsApiClient;
        private readonly ILogger<CartService> _logger;

        public CartService(
         ICartRepository repository,
         IProductsApiClient productsApiClient,
         ILogger<CartService> logger)
        {
            _repository = repository;
            _productsApiClient = productsApiClient;
            _logger = logger;
        }


        // GET /api/cart/{userId}
        public async Task<CartResponseDto> GetCartAsync(Guid userId)
        {
            // Busca el carrito activo del usuario.
            var cart = await _repository.GetByUserIdAsync(userId);

            // Según el TP, si no tiene carrito activo corresponde CRT-001.
            if (cart is null)
                throw new NotFoundException(
                    CartErrorCodes.CartNotFound,
                    "Carrito no encontrado.");

            return MapToResponse(cart);
        }

        // POST /api/cart/{userId}/items
        public async Task<CartResponseDto> AddItemAsync(Guid userId, AddCartItemRequestDto request)
        {
            // Valida cantidad mayor a cero.
            if (request.Cantidad <= 0)
                throw new ValidationException(
                    CartErrorCodes.InvalidQuantity,
                    "Cantidad inválida.");

            // Valida que el producto exista en Products.API.
            var product = await _productsApiClient.GetProductByIdAsync(request.ProductoId);

            if (product is null)
                throw new NotFoundException(
                    CartErrorCodes.ProductNotFound,
                    "Producto no encontrado.");

            // Si el producto ya estaba en el carrito, se suma la nueva cantidad.
            var existingQuantity = await _repository.GetItemQuantityAsync(userId, request.ProductoId);
            var finalQuantity = (existingQuantity ?? 0) + request.Cantidad;

            // Valida stock suficiente contra Products.API.
            if (finalQuantity > product.Stock)
                throw new BusinessRuleException(
                    CartErrorCodes.InsufficientStock,
                    $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {finalQuantity}.");

            // Inserta o actualiza el item.
            await _repository.UpsertItemAsync(
                userId,
                request.ProductoId,
                finalQuantity,
                DateTime.UtcNow);

            _logger.LogInformation(
                "Producto agregado al carrito. UsuarioId: {UsuarioId}, ProductoId: {ProductoId}, CantidadFinal: {CantidadFinal}",
                userId,
                request.ProductoId,
                finalQuantity);

            return await GetCartAsync(userId);
        }

        // PUT /api/cart/{userId}/items/{productId}
        public async Task<CartResponseDto> UpdateItemAsync(
       Guid userId,
       Guid productId,
       UpdateCartItemRequestDto request)
        {
            // Valida cantidad mayor a cero.
            if (request.Cantidad <= 0)
                throw new ValidationException(
                    CartErrorCodes.InvalidQuantity,
                    "Cantidad inválida.");

            // CRT-001 si el userId no tiene carrito activo.
            var cartExists = await _repository.CartExistsAsync(userId);

            if (!cartExists)
                throw new NotFoundException(
                    CartErrorCodes.CartNotFound,
                    "Carrito no encontrado.");

            
            // se reutiliza CRT-001 dentro del caso 404 de carrito/item no disponible.
            var itemExists = await _repository.ItemExistsAsync(userId, productId);

            if (!itemExists)
                throw new NotFoundException(
                    CartErrorCodes.CartNotFound,
                    "Carrito no encontrado.");

            // Valida que el producto exista en Products.API.
            var product = await _productsApiClient.GetProductByIdAsync(productId);

            if (product is null)
                throw new NotFoundException(
                    CartErrorCodes.ProductNotFound,
                    "Producto no encontrado.");

            // Valida stock suficiente.
            if (request.Cantidad > product.Stock)
                throw new BusinessRuleException(
                    CartErrorCodes.InsufficientStock,
                    $"Stock insuficiente. Disponible: {product.Stock}, solicitado: {request.Cantidad}.");

            // Actualiza la cantidad del item.
            await _repository.UpdateItemAsync(
                userId,
                productId,
                request.Cantidad,
                DateTime.UtcNow);

            _logger.LogInformation(
                "Cantidad de item actualizada. UsuarioId: {UsuarioId}, ProductoId: {ProductoId}, Cantidad: {Cantidad}",
                userId,
                productId,
                request.Cantidad);

            return await GetCartAsync(userId);
        }

        // DELETE /api/cart/{userId}/items/{productId}
        public async Task DeleteItemAsync(Guid userId, Guid productId)
        {
            // Valida que exista carrito activo.
            var cartExists = await _repository.CartExistsAsync(userId);

            if (!cartExists)
                throw new NotFoundException(
                    CartErrorCodes.CartNotFound,
                    "Carrito no encontrado.");

            // Valida que exista el item dentro del carrito.
            var itemExists = await _repository.ItemExistsAsync(userId, productId);

            if (!itemExists)
                throw new NotFoundException(
                    CartErrorCodes.CartNotFound,
                    "Carrito no encontrado.");

            await _repository.DeleteItemAsync(userId, productId);

            _logger.LogInformation(
                "Producto quitado del carrito. UsuarioId: {UsuarioId}, ProductoId: {ProductoId}",
                userId,
                productId);
        }

        // DELETE /api/cart/{userId}
        public async Task ClearCartAsync(Guid userId)
        {
            // Valida que exista carrito activo.
            var cartExists = await _repository.CartExistsAsync(userId);

            if (!cartExists)
                throw new NotFoundException(
                    CartErrorCodes.CartNotFound,
                    "Carrito no encontrado.");

            await _repository.ClearCartAsync(userId);

            _logger.LogInformation(
                "Carrito vaciado. UsuarioId: {UsuarioId}",
                userId);
        }

        private static CartResponseDto MapToResponse(Model.Cart cart)
        {
            // Mapea entidad de dominio a DTO de salida.
            return new CartResponseDto
            {
                UsuarioId = cart.UsuarioId,
                FechaActualizacion = cart.FechaActualizacion,
                Items = cart.Items.Select(item => new CartItemResponseDto
                {
                    ProductoId = item.ProductoId,
                    Cantidad = item.Cantidad
                }).ToList()
            };
        }
    }
}
