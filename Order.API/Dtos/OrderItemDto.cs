using System.ComponentModel.DataAnnotations;

namespace Orders.API.Dtos;

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
}