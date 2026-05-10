namespace ECommerce_G24.source.Products.API.Dtos
{
    public class ProductResponseDto
    {
        public Guid Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public string? Descripcion { get; init; }
        public decimal Precio { get; init; }
        public int Stock { get; init; }
        public string Categoria { get; init; } = string.Empty;
        public DateTime FechaCreacion { get; init; }
    }
}
