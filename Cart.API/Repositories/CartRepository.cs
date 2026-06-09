using Cart.API.Cart.API.Model;
using CartModel = Cart.API.Cart.API.Model.Cart;
using Dapper;
using Microsoft.Data.Sqlite;
using System.Globalization;

namespace Cart.API.Cart.API.Repositories
{
    /// <summary>
    /// Implementación SQLite + Dapper del repositorio de carritos.
    /// </summary>
    public class CartRepository : ICartRepository
    {
        private readonly IConfiguration _configuration;

        public CartRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqliteConnection CreateConnection()
        {
            // Lee la cadena de conexión desde appsettings.json.
            var connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=cart.db";

            return new SqliteConnection(connectionString);
        }

        public async Task<CartModel?> GetByUserIdAsync(Guid userId)
        {
            using var connection = CreateConnection();

            var items = (await connection.QueryAsync<CartItemRecord>("""
                SELECT
                    producto_id AS ProductoId,
                    cantidad AS Cantidad,
                    fecha_actualizacion AS FechaActualizacion
                FROM cart_items
                WHERE usuario_id = @UsuarioId
                ORDER BY fecha_actualizacion DESC;
            """, new
            {
                UsuarioId = userId.ToString()
            })).ToList();

            if (items.Count == 0)
                return null;

            var cartItems = items.Select(item => new CartItem
            {
                ProductoId = Guid.Parse(item.ProductoId),
                Cantidad = item.Cantidad
            }).ToList();

            var ultimaFechaActualizacion = items
                .Select(item => DateTime.Parse(
                    item.FechaActualizacion,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind))
                .Max();

            return new CartModel
            {
                UsuarioId = userId,
                Items = cartItems,
                FechaActualizacion = ultimaFechaActualizacion
            };
        }

        public async Task<bool> CartExistsAsync(Guid userId)
        {
            using var connection = CreateConnection();

            var count = await connection.ExecuteScalarAsync<int>("""
                SELECT COUNT(1)
                FROM cart_items
                WHERE usuario_id = @UsuarioId;
            """, new
            {
                UsuarioId = userId.ToString()
            });

            return count > 0;
        }

        public async Task<bool> ItemExistsAsync(Guid userId, Guid productId)
        {
            using var connection = CreateConnection();

            var count = await connection.ExecuteScalarAsync<int>("""
                SELECT COUNT(1)
                FROM cart_items
                WHERE usuario_id = @UsuarioId
                  AND producto_id = @ProductoId;
            """, new
            {
                UsuarioId = userId.ToString(),
                ProductoId = productId.ToString()
            });

            return count > 0;
        }

        public async Task<int?> GetItemQuantityAsync(Guid userId, Guid productId)
        {
            using var connection = CreateConnection();

            return await connection.QuerySingleOrDefaultAsync<int?>("""
                SELECT cantidad
                FROM cart_items
                WHERE usuario_id = @UsuarioId
                  AND producto_id = @ProductoId;
            """, new
            {
                UsuarioId = userId.ToString(),
                ProductoId = productId.ToString()
            });
        }

        public async Task UpsertItemAsync(
            Guid userId,
            Guid productId,
            int quantity,
            DateTime updatedAt)
        {
            using var connection = CreateConnection();

            await connection.ExecuteAsync("""
                INSERT INTO cart_items (
                    usuario_id,
                    producto_id,
                    cantidad,
                    fecha_actualizacion
                )
                VALUES (
                    @UsuarioId,
                    @ProductoId,
                    @Cantidad,
                    @FechaActualizacion
                )
                ON CONFLICT(usuario_id, producto_id)
                DO UPDATE SET
                    cantidad = excluded.cantidad,
                    fecha_actualizacion = excluded.fecha_actualizacion;
            """, new
            {
                UsuarioId = userId.ToString(),
                ProductoId = productId.ToString(),
                Cantidad = quantity,
                FechaActualizacion = updatedAt.ToString("O", CultureInfo.InvariantCulture)
            });
        }

        public async Task UpdateItemAsync(
            Guid userId,
            Guid productId,
            int quantity,
            DateTime updatedAt)
        {
            using var connection = CreateConnection();

            await connection.ExecuteAsync("""
                UPDATE cart_items
                SET
                    cantidad = @Cantidad,
                    fecha_actualizacion = @FechaActualizacion
                WHERE usuario_id = @UsuarioId
                  AND producto_id = @ProductoId;
            """, new
            {
                UsuarioId = userId.ToString(),
                ProductoId = productId.ToString(),
                Cantidad = quantity,
                FechaActualizacion = updatedAt.ToString("O", CultureInfo.InvariantCulture)
            });
        }

        public async Task DeleteItemAsync(Guid userId, Guid productId)
        {
            using var connection = CreateConnection();

            await connection.ExecuteAsync("""
                DELETE FROM cart_items
                WHERE usuario_id = @UsuarioId
                  AND producto_id = @ProductoId;
            """, new
            {
                UsuarioId = userId.ToString(),
                ProductoId = productId.ToString()
            });
        }

        public async Task ClearCartAsync(Guid userId)
        {
            using var connection = CreateConnection();

            await connection.ExecuteAsync("""
                DELETE FROM cart_items
                WHERE usuario_id = @UsuarioId;
            """, new
            {
                UsuarioId = userId.ToString()
            });
        }

        /// <summary>
        /// Clase auxiliar para mapear filas de SQLite.
        /// SQLite guarda Guid y DateTime como TEXT, por eso primero se leen como string
        /// y luego se convierten manualmente.
        /// </summary>
        private class CartItemRecord
        {
            public string ProductoId { get; set; } = string.Empty;

            public int Cantidad { get; set; }

            public string FechaActualizacion { get; set; } = string.Empty;
        }
    }
}