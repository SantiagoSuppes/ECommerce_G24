using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

/// <summary>
/// Datos requeridos para actualizar el estado.
/// </summary>
public class UpdateOrderStatusRequestDto
{
    /// <summary>
    /// Nuevo estado de la orden.
    /// </summary>
    [Required(
        ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = string.Empty;
}