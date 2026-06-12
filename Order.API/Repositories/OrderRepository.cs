using System.Globalization;
using Dapper;
using Microsoft.Data.Sqlite;
using Orders.API.Models;

namespace Orders.API.Repositories;

/// <summary>
/// Implementación SQLite + Dapper
/// del repositorio de órdenes.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly IConfiguration _configuration;

    public OrderRepository(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private SqliteConnection CreateConnection()
    {
        var connectionString =
            _configuration.GetConnectionString(
                "DefaultConnection")
            ?? "Data Source=orders.db";

        return new SqliteConnection(connectionString);
    }

    public async Task<IReadOnlyCollection<Order>> GetAllAsync(
        Guid? userId)
    {
        using var connection = CreateConnection();

        const string sql = """
            SELECT
                id AS Id,
                usuario_id AS UsuarioId,
                total AS Total,
                estado AS Estado,
                fecha_creacion AS FechaCreacion,
                fecha_actualizacion AS FechaActualizacion
            FROM orders
            WHERE
                @UsuarioId IS NULL
                OR usuario_id = @UsuarioId
            ORDER BY fecha_creacion DESC;
        """;

        var orderRecords =
            (await connection.QueryAsync<OrderRecord>(
                sql,
                new
                {
                    UsuarioId =
                        userId?.ToString()
                }))
            .ToList();

        if (orderRecords.Count == 0)
            return Array.Empty<Order>();

        var orderIds =
            orderRecords
                .Select(record => record.Id)
                .ToArray();

        const string itemSql = """
            SELECT
                order_id AS OrderId,
                producto_id AS ProductoId,
                cantidad AS Cantidad,
                precio_unitario AS PrecioUnitario
            FROM order_items
            WHERE order_id IN @OrderIds;
        """;

        var itemRecords =
            (await connection.QueryAsync<OrderItemRecord>(
                itemSql,
                new
                {
                    OrderIds = orderIds
                }))
            .ToList();

        var itemsByOrder =
            itemRecords
                .GroupBy(item => item.OrderId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(MapToOrderItem)
                        .ToList());

        return orderRecords
            .Select(record =>
                MapToOrder(
                    record,
                    itemsByOrder.TryGetValue(
                        record.Id,
                        out var items)
                        ? items
                        : new List<OrderItem>()))
            .ToList();
    }

    public async Task<Order?> GetByIdAsync(
        Guid orderId)
    {
        using var connection = CreateConnection();

        const string orderSql = """
            SELECT
                id AS Id,
                usuario_id AS UsuarioId,
                total AS Total,
                estado AS Estado,
                fecha_creacion AS FechaCreacion,
                fecha_actualizacion AS FechaActualizacion
            FROM orders
            WHERE id = @Id;
        """;

        var record =
            await connection.QuerySingleOrDefaultAsync<OrderRecord>(
                orderSql,
                new
                {
                    Id = orderId.ToString()
                });

        if (record is null)
            return null;

        const string itemSql = """
            SELECT
                order_id AS OrderId,
                producto_id AS ProductoId,
                cantidad AS Cantidad,
                precio_unitario AS PrecioUnitario
            FROM order_items
            WHERE order_id = @OrderId
            ORDER BY producto_id;
        """;

        var itemRecords =
            await connection.QueryAsync<OrderItemRecord>(
                itemSql,
                new
                {
                    OrderId = orderId.ToString()
                });

        var items =
            itemRecords
                .Select(MapToOrderItem)
                .ToList();

        return MapToOrder(
            record,
            items);
    }

    public async Task<Order> CreateAsync(
        Order order)
    {
        using var connection = CreateConnection();

        await connection.OpenAsync();

        using var transaction =
            connection.BeginTransaction();

        try
        {
            const string orderSql = """
                INSERT INTO orders (
                    id,
                    usuario_id,
                    total,
                    estado,
                    fecha_creacion,
                    fecha_actualizacion
                )
                VALUES (
                    @Id,
                    @UsuarioId,
                    @Total,
                    @Estado,
                    @FechaCreacion,
                    NULL
                );
            """;

            await connection.ExecuteAsync(
                orderSql,
                new
                {
                    Id = order.Id.ToString(),

                    UsuarioId =
                        order.UsuarioId.ToString(),

                    order.Total,
                    order.Estado,

                    FechaCreacion =
                        order.FechaCreacion.ToString("O")
                },
                transaction);

            const string itemSql = """
                INSERT INTO order_items (
                    order_id,
                    producto_id,
                    cantidad,
                    precio_unitario
                )
                VALUES (
                    @OrderId,
                    @ProductoId,
                    @Cantidad,
                    @PrecioUnitario
                );
            """;

            foreach (var item in order.Items)
            {
                await connection.ExecuteAsync(
                    itemSql,
                    new
                    {
                        OrderId =
                            order.Id.ToString(),

                        ProductoId =
                            item.ProductoId.ToString(),

                        item.Cantidad,
                        item.PrecioUnitario
                    },
                    transaction);
            }

            transaction.Commit();

            return order;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task UpdateStatusAsync(
        Guid orderId,
        string status,
        DateTime updatedAt)
    {
        using var connection = CreateConnection();

        await connection.ExecuteAsync("""
            UPDATE orders
            SET
                estado = @Estado,
                fecha_actualizacion = @FechaActualizacion
            WHERE id = @Id;
        """, new
        {
            Id = orderId.ToString(),
            Estado = status,

            FechaActualizacion =
                updatedAt.ToString("O")
        });
    }

    private static Order MapToOrder(
        OrderRecord record,
        List<OrderItem> items)
    {
        return new Order
        {
            Id = Guid.Parse(record.Id),

            UsuarioId =
                Guid.Parse(record.UsuarioId),

            Items = items,

            Total =
                Convert.ToDecimal(record.Total),

            Estado = record.Estado,

            FechaCreacion =
                DateTime.Parse(
                    record.FechaCreacion,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind),

            FechaActualizacion =
                string.IsNullOrWhiteSpace(
                    record.FechaActualizacion)
                    ? null
                    : DateTime.Parse(
                        record.FechaActualizacion,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind)
        };
    }

    private static OrderItem MapToOrderItem(
        OrderItemRecord record)
    {
        return new OrderItem
        {
            ProductoId =
                Guid.Parse(record.ProductoId),

            Cantidad =
                record.Cantidad,

            PrecioUnitario =
                Convert.ToDecimal(
                    record.PrecioUnitario)
        };
    }

    private sealed class OrderRecord
    {
        public string Id { get; set; } =
            string.Empty;

        public string UsuarioId { get; set; } =
            string.Empty;

        public double Total { get; set; }

        public string Estado { get; set; } =
            string.Empty;

        public string FechaCreacion { get; set; } =
            string.Empty;

        public string? FechaActualizacion { get; set; }
    }

    private sealed class OrderItemRecord
    {
        public string OrderId { get; set; } =
            string.Empty;

        public string ProductoId { get; set; } =
            string.Empty;

        public int Cantidad { get; set; }

        public double PrecioUnitario { get; set; }
    }
}