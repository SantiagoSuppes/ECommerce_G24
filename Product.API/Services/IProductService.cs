using ECommerce_G24.Products.API.Dtos;

namespace ECommerce_G24.Products.API.Services
{
    // Contrato de la capa de negocio de productos.
    public interface IProductService
    {
        // Lista productos con filtros opcionales.
        Task<IEnumerable<ProductResponseDto>> GetAllAsync(string? nombre, string? categoria);

        // Obtiene un producto por ID.
        Task<ProductResponseDto> GetByIdAsync(Guid id);

        // Crea un producto nuevo.
        Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request);

        // Actualiza un producto existente.
        Task<ProductResponseDto> UpdateAsync(Guid id, UpdateProductRequestDto request);

        // Elimina un producto.
        Task DeleteAsync(Guid id);
    }
}
