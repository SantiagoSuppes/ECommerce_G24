namespace ECommerce_G24.Cart.API.Model
{
    // Representa el carrito de un usuario.
    public class Cart
    {
        // Usuario dueño del carrito.
        public Guid UsuarioId { get; set; }
        //Lista de prodcutos agregados al carrito
        public List<CartItem> Items { get; set; } = new();
        //Fecha en la que se modificó por última vez
        public DateTime FechaActualizacion { get; set; }
    }
}
