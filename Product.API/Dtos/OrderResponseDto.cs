namespace ECommerce_G24.Products.API.Dtos
{
    /// <summary>
    /// Representa una orden obtenida desde Orders.API.
    /// </summary>
    public class OrderResponseDto
    {
        public Guid Id { get; set; }

        public Guid UsuarioId { get; set; }

        public List<OrderItemResponseDto> Items { get; set; } = [];

        public decimal Total { get; set; }

        public string Estado { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }
    }
}
