using ECommerce_G24.Cart.Api.Dtos;
using ECommerce_G24.Cart.Api.Exceptions;
using ECommerce_G24.Cart.Api.Model;

namespace ECommerce_G24.Cart.Api.Services
{
    public class CartService
    {
        private static readonly List<ECommerce_G24.Cart.Api.Model.Cart> _carts = new();

        public Task<CartResponseDto> GetCartAsync(Guid userId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            return Task.FromResult(MapToResponse(cart));
        }

        public Task<CartResponseDto> AddItemAsync(Guid userId, AddCartItemRequestDto request)
        {
            if (request.Cantidad <= 0)
                throw new ValidationException("CRT-004", "Cantidad inválida.");

            

            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

            if (cart == null)
            {
                cart = new ECommerce_G24.Cart.Api.Model.Cart
                {
                    UsuarioId = userId,
                    FechadeActualizacion = DateTime.UtcNow
                };

                _carts.Add(cart);
            }

            var item = cart.Items.FirstOrDefault(i => i.ProductoId == request.ProductoId);

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
                item.Cantidad += request.Cantidad;
            }

            cart.FechadeActualizacion = DateTime.UtcNow;

            return Task.FromResult(MapToResponse(cart));
        }

        public Task<CartResponseDto> UpdateItemAsync(Guid userId, Guid productId, UpdateCartItemRequestDto request)
        {
            if (request.Cantidad <= 0)
                throw new ValidationException("CRT-004", "Cantidad inválida.");

            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId);

            if (item == null)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            item.Cantidad = request.Cantidad;
            cart.FechadeActualizacion = DateTime.UtcNow;

            return Task.FromResult(MapToResponse(cart));
        }

        public Task DeleteItemAsync(Guid userId, Guid productId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            var item = cart.Items.FirstOrDefault(i => i.ProductoId == productId);

            if (item == null)
                throw new NotFoundException("CRT-002", "Producto no encontrado.");

            cart.Items.Remove(item);
            cart.FechadeActualizacion = DateTime.UtcNow;

            return Task.CompletedTask;
        }

        public Task ClearCartAsync(Guid userId)
        {
            var cart = _carts.FirstOrDefault(c => c.UsuarioId == userId);

            if (cart == null)
                throw new NotFoundException("CRT-001", "Carrito no encontrado.");

            _carts.Remove(cart);

            return Task.CompletedTask;
        }

        private static CartResponseDto MapToResponse(Cart.Api.Model.Cart cart)
        {
            return new CartResponseDto
            {
                UsuarioId = cart.UsuarioId,
                FechaActualizacion = cart.FechadeActualizacion,
                Items = cart.Items.Select(i => new CartItemResponseDto
                {
                    ProductoId = i.ProductoId,
                    Cantidad = i.Cantidad
                }).ToList()
            };
        }
    }
}
