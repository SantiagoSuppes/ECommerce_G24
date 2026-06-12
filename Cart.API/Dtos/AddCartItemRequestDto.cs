using System.ComponentModel.DataAnnotations;

namespace Cart.API.Cart.API.Dtos
{
    // DTO que recibe el POST para agregar un producto al carrito.
    public class AddCartItemRequestDto
    {
        // Producto que se quiere agregar.
        [Required(ErrorMessage = "El producto es obligatorio.")]
        public Guid ProductoId { get; set; }

        // Cantidad a agregar.
        // Debe ser mayor a cero.
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }
    }
}
