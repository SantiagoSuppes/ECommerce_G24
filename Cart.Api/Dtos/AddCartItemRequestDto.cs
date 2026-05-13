using System.ComponentModel.DataAnnotations;

namespace ECommerce_G24.Cart.Api.Dtos
{
    public class AddCartItemRequestDto
    {
        [Required]
        public Guid ProductoId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
        public int Cantidad { get; set; }
    }
}
