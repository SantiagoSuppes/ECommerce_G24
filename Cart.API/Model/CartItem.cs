namespace Cart.API.Cart.API.Model
{
    // Representa un producto dentro del carrito.
    public class CartItem
    {
        // ID del producto. Este ID viene de Products.API.
        public Guid ProductoId { get; set; }

        // Cantidad del producto en el carrito.
        public int Cantidad { get; set; }
    }
}
