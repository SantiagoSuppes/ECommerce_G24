namespace ECommerce_G24.Products.API.Dtos
{
    namespace Products.API.DTOs
    {
        // DTO que se devuelve como respuesta de producto.
        public class ProductResponseDto
        {
            // Identificador único del producto.
            public Guid Id { get; set; }

            // Nombre del producto.
            public string Nombre { get; set; } = string.Empty;

            // Descripción del producto.
            public string? Descripcion { get; set; }

            // Precio del producto.
            public decimal Precio { get; set; }

            // Stock disponible del producto.
            public int Stock { get; set; }

            // Categoría del producto.
            public string Categoria { get; set; } = string.Empty;

            // Fecha en la que fue creado el producto.
            public DateTime FechaCreacion { get; set; }
        }
    }
}
