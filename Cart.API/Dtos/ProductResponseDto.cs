namespace ECommerce_G24.Cart.API.Dtos
{
    // DTO usado internamente para leer datos desde Products.API.
    // Cart.API necesita consultar si el producto existe y cuánto stock tiene.
    public class ProductResponseDto
    {
        public Guid Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public string Categoria { get; set; } = string.Empty;

        public DateTime FechaCreacion { get; set; }
    }
}
