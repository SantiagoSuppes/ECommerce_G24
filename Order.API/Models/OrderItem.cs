using System.ComponentModel.DataAnnotations;

namespace Orders.API.Models;

/// <summary>
/// Representa un producto específico dentro de una orden.
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Identificador único del item de la orden.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador del producto comprado.
    /// </summary>
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Cantidad comprada.
    /// </summary>
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio del producto al momento de crear la orden.
    /// </summary>
    public decimal PrecioUnitario { get; set; }
}