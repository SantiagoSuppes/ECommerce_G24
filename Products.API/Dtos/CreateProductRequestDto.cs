using System.ComponentModel.DataAnnotations;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ECommerce_G24.Products.API.Dtos
{
    public class CreateProductRequestDto
    {
        [Required(ErrorMessage = "El campo nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El campo nombre tiene un máximo de 100 caracteres")]
        public string Nombre { get; init; } = string.Empty;
        [MaxLength(500, ErrorMessage = "El campo descripción tiene un máximo de 500 caracteres")]
        public string? Descripcion { get; init; }
        [Required(ErrorMessage = "El campo precio es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El campo precio no pude ser negativo")]
        public decimal Precio { get; init; }
        [Required(ErrorMessage ="El campo stock es obligatorio")]
        [Range(0,int.MaxValue,ErrorMessage ="El campo stock no puede ser negativo")]
        public int Stock { get; init; }
        [Required(ErrorMessage = "El campo categoria es obligatorio")]
        public string Categoria { get; init; } = string.Empty;



    }
}




