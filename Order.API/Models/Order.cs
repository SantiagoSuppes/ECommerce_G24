namespace Orders.API.Models;

//Representa una orden de compra realizada por un usuario, incluyendo los detalles de los productos comprados, el total de la orden, su estado y la fecha de creación.
public class Order
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}