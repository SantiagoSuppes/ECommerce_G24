using System.ComponentModel.DataAnnotations;

namespace ECommerce_G24.Products.API.Models
{
    
        // Entidad de dominio que representa un producto.
        public class Product
        {
            // Identificador único del producto.
            public Guid Id { get; init; }

            // Nombre del producto. Obligatorio y Maximo 100 caracteres.
            [Required]
            [MaxLength(100)]
            public string Nombre { get; set; } = string.Empty;

            // Descripción del producto. Maximo 500 caracteres
            [MaxLength(500)]
            public string? Descripcion { get; set; }

            // Precio del producto. Mayor a 0.
            [Range(0.01, double.MaxValue)]
            public decimal Precio { get; set; }

            // Stock obligatorio. Debe ser mayor o igual a cero.
            [Range(0, int.MaxValue)]
            public int Stock { get; set; }

            // Categoría obligatoria. El TP aclara que no hace falta validar contra una lista cerrada.
            [Required]
            public string Categoria { get; set; } = string.Empty;

            // Fecha asignada automáticamente al momento de crear el producto.
            public DateTime FechaCreacion { get; set; }
        }
}





    

