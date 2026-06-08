using ECommerce_G24.Products.API.Models;

namespace ECommerce_G24.Products.API.Repositories
{
    // Contrato de persistencia para productos.
    // La capa de servicios lo usa para no depender directamente de SQLite/Dapper.
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(string? nombre, string? categoria);

        Task<Product?> GetByIdAsync(Guid id);

        Task<Product?> GetByNameAndCategoryAsync(string nombre, string categoria);

        Task<Product> CreateAsync(Product product);

        Task<Product> UpdateAsync(Product product);

        Task DeleteAsync(Guid id);
    }
}
