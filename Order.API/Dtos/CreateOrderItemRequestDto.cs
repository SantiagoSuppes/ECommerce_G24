using System.ComponentModel.DataAnnotations;

namespace Orders.API.DTOs;

/// <summary>
/// Producto solicitado al crear una orden.
/// </summary>
public class CreateOrderItemRequestDto
{
    /// <summary>
    /// Identificador del producto.
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Cantidad solicitada.
    /// </summary>
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }
}