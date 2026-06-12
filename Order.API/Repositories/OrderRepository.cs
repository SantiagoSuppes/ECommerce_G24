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

    /// <summary>
    /// Crea una conexión nueva a SQLite.
    /// </summary>
    private SqliteConnection CreateConnection()
    {
        var connectionString =
            _configuration.GetConnectionString(
                "DefaultConnection")
            ?? "Data Source=orders.db";

        return new SqliteConnection(connectionString);
    }

    /// <summary>
    /// Obtiene todas las órdenes.
    /// Si se informa userId, filtra por usuario.
    /// </summary>
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
                    UsuarioId = userId?.ToString()
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
                id AS Id,
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

    /// <summary>
    /// Obtiene una orden por ID, incluyendo sus items.
    /// </summary>
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
                id AS Id,
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

    /// <summary>
    /// Crea una orden junto con sus items.
    /// Usa transacción para evitar guardar una cabecera
    /// sin detalle o un detalle sin cabecera.
    /// </summary>
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
                    id,
                    order_id,
                    producto_id,
                    cantidad,
                    precio_unitario
                )
                VALUES (
                    @Id,
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
                        Id =
                            item.Id.ToString(),

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

    /// <summary>
    /// Actualiza el estado de una orden.
    /// </summary>
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

    /// <summary>
    /// Convierte un registro de cabecera
    /// y su lista de items al modelo Order.
    /// </summary>
    private static Order MapToOrder(
        OrderRecord record,
        List<OrderItem> items)
    {
        return new Order
        {
            Id =
                Guid.Parse(record.Id),

            UsuarioId =
                Guid.Parse(record.UsuarioId),

            Items =
                items,

            Total =
                Convert.ToDecimal(record.Total),

            Estado =
                record.Estado,

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

    /// <summary>
    /// Convierte un registro de SQLite
    /// al modelo OrderItem.
    /// </summary>
    private static OrderItem MapToOrderItem(
        OrderItemRecord record)
    {
        return new OrderItem
        {
            Id =
                Guid.Parse(record.Id),

            ProductoId =
                Guid.Parse(record.ProductoId),

            Cantidad =
                record.Cantidad,

            PrecioUnitario =
                Convert.ToDecimal(
                    record.PrecioUnitario)
        };
    }

    /// <summary>
    /// Registro auxiliar para leer la cabecera
    /// de órdenes desde SQLite.
    /// </summary>
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

    /// <summary>
    /// Registro auxiliar para leer items
    /// desde SQLite.
    /// </summary>
    private sealed class OrderItemRecord
    {
        public string Id { get; set; } =
            string.Empty;

        public string OrderId { get; set; } =
            string.Empty;

        public string ProductoId { get; set; } =
            string.Empty;

        public int Cantidad { get; set; }

        public double PrecioUnitario { get; set; }
    }
}