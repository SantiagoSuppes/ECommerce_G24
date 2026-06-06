using System.ComponentModel.DataAnnotations;

namespace ECommerce_G24.Orders.API.Dtos;

/// <summary>
/// DTO para un item de la orden.
/// </summary>
public class OrderItemDto
{
    /// <summary>
    /// ID del producto.
    /// </summary>
    [Required(ErrorMessage = "El ID del producto es requerido.")]
    public Guid ProductoId { get; set; }

    /// <summary>
    /// Cantidad del producto.
    /// </summary>
    [Required(ErrorMessage = "La cantidad es requerida.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    /// <summary>
    /// Precio unitario del producto.
    /// </summary>
    [Required(ErrorMessage = "El precio unitario es requerido.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor a 0.")]
    public decimal PrecioUnitario { get; set; }
}