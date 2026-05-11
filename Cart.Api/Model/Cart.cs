namespace ECommerce_G24.Cart.Api.Model
{
    public class Cart
    {
        public Guid UsuarioId { get; init; }
        public List<CartItem> Items { get; set; } = new();
        public DateTime FechadeActualizacion { get; set; }
    }
}
