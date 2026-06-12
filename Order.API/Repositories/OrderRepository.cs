using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;

namespace Orders.API.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IConfiguration _configuration;

    public OrderRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqliteConnection CreateConnection()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=orders.db";
        return new SqliteConnection(connectionString);
    }

    public async Task<IEnumerable<Order>> GetAllAsync(string? usuarioId)
    {
        using var connection = CreateConnection();

        var records = await connection.QueryAsync<OrderRecord>(
            usuarioId == null
                ? "SELECT id, usuario_id, total, estado, fecha_creacion FROM orders"
                : "SELECT id, usuario_id, total, estado, fecha_creacion FROM orders WHERE usuario_id = @UsuarioId",
            new { UsuarioId = usuarioId });

        var result = new List<Order>();
        foreach (var record in records)
        {
            var items = await GetItemsByOrderIdAsync(connection, record.Id);
            result.Add(MapToOrder(record, items));
        }

        return result;
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        using var connection = CreateConnection();

        var record = await connection.QueryFirstOrDefaultAsync<OrderRecord>(
            "SELECT id, usuario_id, total, estado, fecha_creacion FROM orders WHERE id = @Id",
            new { Id = id.ToString() });

        if (record == null)
            return null;

        var items = await GetItemsByOrderIdAsync(connection, record.Id);
        return MapToOrder(record, items);
    }

    public async Task<Order> CreateAsync(Order order, List<OrderItem> items)
    {
        using var connection = CreateConnection();

        await connection.ExecuteAsync(
            "INSERT INTO orders (id, usuario_id, total, estado, fecha_creacion) VALUES (@Id, @UsuarioId, @Total, @Estado, @FechaCreacion)",
            new
            {
                Id = order.Id.ToString(),
                order.UsuarioId,
                order.Total,
                order.Estado,
                FechaCreacion = order.FechaCreacion.ToString("o")
            });

        foreach (var item in items)
        {
            await connection.ExecuteAsync(
                "INSERT INTO order_items (id, order_id, producto_id, cantidad, precio_unitario) VALUES (@Id, @OrderId, @ProductoId, @Cantidad, @PrecioUnitario)",
                new
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderId = order.Id.ToString(),
                    ProductoId = item.ProductoId.ToString(),
                    item.Cantidad,
                    item.PrecioUnitario
                });
        }

        return (await GetByIdAsync(order.Id))!;
    }

    public async Task UpdateStatusAsync(Guid id, string estado)
    {
        using var connection = CreateConnection();

        await connection.ExecuteAsync(
            "UPDATE orders SET estado = @Estado WHERE id = @Id",
            new { Estado = estado, Id = id.ToString() });
    }

    private static async Task<List<OrderItem>> GetItemsByOrderIdAsync(SqliteConnection connection, string orderId)
    {
        var records = await connection.QueryAsync<OrderItemRecord>(
            "SELECT producto_id, cantidad, precio_unitario FROM order_items WHERE order_id = @OrderId",
            new { OrderId = orderId });

        return records.Select(r => new OrderItem
        {
            ProductoId = Guid.Parse(r.ProductoId),
            Cantidad = r.Cantidad,
            PrecioUnitario = r.PrecioUnitario
        }).ToList();
    }

    private static Order MapToOrder(OrderRecord record, List<OrderItem> items)
    {
        return new Order
        {
            Id = Guid.Parse(record.Id),
            UsuarioId = record.UsuarioId,
            Items = items,
            Total = record.Total,
            Estado = record.Estado,
            FechaCreacion = DateTime.Parse(record.FechaCreacion)
        };
    }

    private class OrderRecord
    {
        public string Id { get; set; } = string.Empty;
        public string UsuarioId { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string FechaCreacion { get; set; } = string.Empty;
    }

    private class OrderItemRecord
    {
        public string ProductoId { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}