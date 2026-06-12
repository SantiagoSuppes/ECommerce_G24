namespace Orders.API.Models;

/// <summary>
/// Representa una orden creada por un usuario.
/// </summary>
public class Order
{
    /// <summary>
    /// Identificador único de la orden.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Usuario que creó la orden.
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Productos incluidos en la orden.
    /// </summary>
    public List<OrderItem> Items { get; set; } = new();

    /// <summary>
    /// Total calculado automáticamente.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Estado actual de la orden.
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Fecha en la que se creó la orden.
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha de la última modificación del estado.
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }
}