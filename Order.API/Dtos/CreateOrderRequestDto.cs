using System.ComponentModel.DataAnnotations;

namespace Orders.API.Dtos;

/// <summary>
/// DTO para la creación de una orden.
/// </summary>
public class CreateOrderRequestDto
{
    /// <summary>
    /// ID del usuario que realiza la orden.
    /// </summary>
    [Required(ErrorMessage = "El ID del usuario es requerido.")]
    [StringLength(36, MinimumLength = 36, ErrorMessage = "El ID del usuario debe ser un GUID válido.")]
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Lista de items de la orden.
    /// </summary>
    [Required(ErrorMessage = "La lista de items es obligatoria.")]
    [MinLength(1, ErrorMessage = "La orden debe tener al menos un item.")]
    public List<OrderItemDto> Items { get; set; } = new();
}