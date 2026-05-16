using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECommerce_G24.Products.API.Models
{
    namespace Products.API.Models
    {
        // Entidad de dominio que representa un producto.
        public class Product
        {
            // Identificador único del producto.
            public Guid Id { get; init; } = Guid.NewGuid();

            // Nombre del producto.
            public string Nombre { get; set; } = string.Empty;

            // Descripción del producto.
            public string? Descripcion { get; set; }

            // Precio del producto.
            public decimal Precio { get; set; }

            // Stock disponible.
            public int Stock { get; set; }

            // Categoría del producto.
            public string Categoria { get; set; } = string.Empty;

            // Fecha de creación.
            public DateTime FechaCreacion { get; init; } = DateTime.UtcNow;
        }
    }
}




    

