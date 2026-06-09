using Dapper;
using ECommerce_G24.Products.API.Models;
using Microsoft.Data.Sqlite;

namespace ECommerce_G24.Products.API.Repositories
{
    /// <summary>
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
            var connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=products.db";

            return new SqliteConnection(connectionString);
        }

        private class ProductDbRow
        {
            public string Id { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public decimal Precio { get; set; }
            public int Stock { get; set; }
            public string Categoria { get; set; } = string.Empty;
            public string FechaCreacion { get; set; } = string.Empty;
        }

        private static Product MapToProduct(ProductDbRow row)
        {
            return new Product
            {
                Id = Guid.Parse(row.Id),
                Nombre = row.Nombre,
                Descripcion = row.Descripcion,
                Precio = row.Precio,
                Stock = row.Stock,
                Categoria = row.Categoria,
                FechaCreacion = DateTime.Parse(row.FechaCreacion)
            };
        }

        public async Task<IEnumerable<Product>> GetAllAsync(string? nombre, string? categoria)
        {
            using var connection = CreateConnection();

            const string sql = """
        SELECT
            Id,
            Nombre,
            Descripcion,
            Precio,
            Stock,
            Categoria,
            FechaCreacion
        FROM Products
        WHERE (@Nombre IS NULL OR LOWER(Nombre) LIKE '%' || LOWER(@Nombre) || '%')
          AND (@Categoria IS NULL OR LOWER(Categoria) = LOWER(@Categoria))
        ORDER BY FechaCreacion DESC;
    """;

            var rows = await connection.QueryAsync<ProductDbRow>(
                sql,
                new
                {
                    Nombre = string.IsNullOrWhiteSpace(nombre) ? null : nombre.Trim(),
                    Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria.Trim()
                }
            );

            return rows.Select(MapToProduct);
        }

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            using var connection = CreateConnection();

            const string sql = """
                SELECT
                    Id,
                    Nombre,
                    Descripcion,
                    Precio,
                    Stock,
                    Categoria,
                    FechaCreacion
                FROM Products
                WHERE Id = @Id;
            """;

            var row = await connection.QuerySingleOrDefaultAsync<ProductDbRow>(
                sql,
                new { Id = id.ToString() }
            );

            return row is null ? null : MapToProduct(row);
        }

        public async Task<Product?> GetByNameAndCategoryAsync(string nombre, string categoria)
        {
            using var connection = CreateConnection();

            const string sql = """
                SELECT
                    Id,
                    Nombre,
                    Descripcion,
                    Precio,
                    Stock,
                    Categoria,
                    FechaCreacion
                FROM Products
                WHERE LOWER(Nombre) = LOWER(@Nombre)
                  AND LOWER(Categoria) = LOWER(@Categoria);
            """;

            var row = await connection.QuerySingleOrDefaultAsync<ProductDbRow>(
                sql,
                new
                {
                    Nombre = nombre.Trim(),
                    Categoria = categoria.Trim()
                }
            );

            return row is null ? null : MapToProduct(row);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            using var connection = CreateConnection();

            const string sql = """
                INSERT INTO Products
                (
                    Id,
                    Nombre,
                    Descripcion,
                    Precio,
                    Stock,
                    Categoria,
                    FechaCreacion
                )
                VALUES
                (
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
                FechaCreacion = product.FechaCreacion.ToString("O")
            });

            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            using var connection = CreateConnection();

            const string sql = """
                UPDATE Products
                SET
                    Nombre = @Nombre,
                    Descripcion = @Descripcion,
                    Precio = @Precio,
                    Stock = @Stock,
                    Categoria = @Categoria
                WHERE Id = @Id;
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

            const string sql = """
                DELETE FROM Products
                WHERE Id = @Id;
            """;

            await connection.ExecuteAsync(sql, new
            {
                Id = id.ToString()
            });
        }
    }
}