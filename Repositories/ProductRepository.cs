using ECommerce_G24.Products.API.Models;
using Microsoft.Data.Sqlite;
using Dapper;

namespace ECommerce_G24.Products.API.Repositories
{
    /// Implementación SQLite + Dapper del repositorio de productos.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly IConfiguration _configuration;

        public ProductRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqliteConnection CreateConnection()
        {
            // Lee la conexión desde appsettings.json.
            var connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=products.db";

            return new SqliteConnection(connectionString);
        }

        public async Task<IEnumerable<Product>> GetAllAsync(string? nombre, string? categoria)
        {
            using var connection = CreateConnection();

            // Consulta base con filtros opcionales por nombre y categoría.
            // Los filtros salen del contrato GET /api/products?categoria=&nombre=.
            var sql = """
            SELECT
                id AS Id,
                nombre AS Nombre,
                descripcion AS Descripcion,
                precio AS Precio,
                stock AS Stock,
                categoria AS Categoria,
                fecha_creacion AS FechaCreacion
            FROM products
            WHERE
                (@Nombre IS NULL OR LOWER(nombre) LIKE '%' || LOWER(@Nombre) || '%')
                AND
                (@Categoria IS NULL OR LOWER(categoria) = LOWER(@Categoria))
            ORDER BY fecha_creacion DESC;
        """;

            return await connection.QueryAsync<Product>(sql, new
            {
                Nombre = string.IsNullOrWhiteSpace(nombre) ? null : nombre.Trim(),
                Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim()
            });
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            using var connection = CreateConnection();

            var sql = """
            SELECT
                id AS Id,
                nombre AS Nombre,
                descripcion AS Descripcion,
                precio AS Precio,
                stock AS Stock,
                categoria AS Categoria,
                fecha_creacion AS FechaCreacion
            FROM products
            WHERE id = @Id;
        """;

            return await connection.QuerySingleOrDefaultAsync<Product>(sql, new
            {
                Id = id.ToString()
            });
        }

        public async Task<Product?> GetByNameAndCategoryAsync(string nombre, string categoria)
        {
            using var connection = CreateConnection();

            var sql = """
            SELECT
                id AS Id,
                nombre AS Nombre,
                descripcion AS Descripcion,
                precio AS Precio,
                stock AS Stock,
                categoria AS Categoria,
                fecha_creacion AS FechaCreacion
            FROM products
            WHERE LOWER(nombre) = LOWER(@Nombre)
              AND LOWER(categoria) = LOWER(@Categoria);
        """;

            return await connection.QuerySingleOrDefaultAsync<Product>(sql, new
            {
                Nombre = nombre.Trim(),
                Categoria = categoria.Trim()
            });
        }

        public async Task<Product> CreateAsync(Product product)
        {
            using var connection = CreateConnection();

            var sql = """
            INSERT INTO products (
                id,
                nombre,
                descripcion,
                precio,
                stock,
                categoria,
                fecha_creacion
            )
            VALUES (
                @Id,
                @Nombre,
                @Descripcion,
                @Precio,
                @Stock,
                @Categoria,
                @FechaCreacion
            );
        """;

            await connection.ExecuteAsync(sql, new
            {
                Id = product.Id.ToString(),
                product.Nombre,
                product.Descripcion,
                product.Precio,
                product.Stock,
                product.Categoria,
                FechaCreacion = product.FechaCreacion
            });

            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            using var connection = CreateConnection();

            var sql = """
            UPDATE products
            SET
                nombre = @Nombre,
                descripcion = @Descripcion,
                precio = @Precio,
                stock = @Stock,
                categoria = @Categoria
            WHERE id = @Id;
        """;

            await connection.ExecuteAsync(sql, new
            {
                Id = product.Id.ToString(),
                product.Nombre,
                product.Descripcion,
                product.Precio,
                product.Stock,
                product.Categoria
            });

            return product;
        }

        public async Task DeleteAsync(Guid id)
        {
            using var connection = CreateConnection();

            var sql = """
            DELETE FROM products
            WHERE id = @Id;
        """;

            await connection.ExecuteAsync(sql, new
            {
                Id = id.ToString()
            });
        }
    }
}
