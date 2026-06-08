using System.ComponentModel.DataAnnotations;

namespace ECommerce_G24.Products.API.Dtos
{
    // DTO usado para crear un producto.
    public class CreateProductRequestDto
    {
        // Nombre del producto.
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        // Descripción opcional del producto.
        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string? Descripcion { get; set; }

        // Precio del producto.
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a cero.")]
        public decimal Precio { get; set; }

        // Stock inicial del producto.
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser mayor o igual a cero.")]
        public int Stock { get; set; }

        // Categoría del producto.
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public string Categoria { get; set; } = string.Empty;
    }
}




