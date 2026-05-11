using ECommerce_G24.Products.API.Dtos;

namespace ECommerce_G24.Products.API.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetAllAsync(string? nombre, string? categoria);
        Task<ProductResponseDto> GetByIDAsync(Guid id);
        Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request);
        Task<ProductResponseDto> UpdateAsync(Guid id, UpdateProductRequestDto request);
        Task<ProductResponseDto> DeleteAsync(Guid id);
    }
}
