using System.ComponentModel.DataAnnotations;

namespace ECommerce_G24.Cart.Api.Dtos
{
    // DTO que recibe el PUT para cambiar la cantidad de un producto.
    public class UpdateCartItemRequestDto
    {
        // Nueva cantidad del producto en el carrito.
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }
    }
}
