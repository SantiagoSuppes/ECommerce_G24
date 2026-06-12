namespace Orders.API.Models;

// Representa un producto específico dentro de una orden, incluyendo su identificador, la cantidad comprada y el precio unitario.
public class OrderItem
{
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
}