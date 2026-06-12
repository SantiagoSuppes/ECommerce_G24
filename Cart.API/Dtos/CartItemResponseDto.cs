namespace Cart.API.Cart.API.Dtos
{
    // DTO que devuelve cada item del carrito.
    public class CartItemResponseDto
    {
        public Guid ProductoId { get; set; }

        public int Cantidad { get; set; }
    }
}
