namespace ECommerce_G24.Products.API.Dtos
{
    /// <summary>
    /// Representa un producto incluido en una orden.
    /// </summary>
    public class OrderItemResponseDto
    {
        public Guid ProductoId { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }
}
