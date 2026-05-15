namespace ECommerce_G24.Cart.API.Dtos
{
    // DTO de respuesta principal del carrito.
    public class CartResponseDto
    {
        public Guid UsuarioId { get; set; }

        public List<CartItemResponseDto> Items { get; set; } = new();

        public DateTime FechaActualizacion { get; set; }
    }
}
