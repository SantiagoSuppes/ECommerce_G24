using System.ComponentModel.DataAnnotations;

namespace ECommerce_G24.Orders.API.Dtos;

/// <summary>
/// DTO para actualizar el estado de una orden.
/// </summary>
public class UpdateOrderStatusRequestDto
{
    /// <summary>
    /// Nuevo estado de la orden (Pendiente, Confirmada, Enviada, Entregada, Cancelada).
    /// </summary>
    [Required(ErrorMessage = "El estado es requerido.")]
    [RegularExpression("Pendiente|Confirmada|Enviada|Entregada|Cancelada",
        ErrorMessage = "El estado debe ser: Pendiente, Confirmada, Enviada, Entregada o Cancelada.")]
    public string Estado { get; set; } = string.Empty;
}