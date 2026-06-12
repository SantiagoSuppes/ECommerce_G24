using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

/// <summary>
/// Datos requeridos para crear una orden.
/// </summary>
public class CreateOrderRequestDto
{
    /// <summary>
    /// Usuario que realiza la orden.
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Productos incluidos en la orden.
    /// Debe contener al menos un item.
    /// </summary>
    [Required(
        ErrorMessage = "La lista de items es obligatoria.")]

    [MinLength(
        1,
        ErrorMessage = "La orden debe contener al menos un item.")]
    public List<CreateOrderItemRequestDto> Items { get; set; } = new();
}