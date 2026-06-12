using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Exceptions;
using  ECommerce_G24.Products.API.Models;
using ECommerce_G24.Products.API.Repositories;

namespace ECommerce_G24.Products.API.Services
{
    // Servicio que contiene la lógica de negocio de productos.
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IOrdersClient _ordersClient;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository repository,
            IOrdersClient ordersClient,
            ILogger<ProductService> logger)
        {
            _repository = repository;
            _ordersClient = ordersClient;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllAsync(string? nombre, string? categoria)
        {
            // Lista productos con filtros opcionales por nombre y categoría.
            var products = await _repository.GetAllAsync(nombre, categoria);

            return products.Select(MapToResponse);
        }

        public async Task<ProductResponseDto> GetByIdAsync(Guid id)
        {
            // Busca el producto por ID.
            var product = await _repository.GetByIdAsync(id);

            // Si no existe, corresponde PRD-001.
            if (product is null)
                throw new NotFoundException(
                    ProductErrorCodes.ProductNotFound,
                    "Producto no encontrado.");

            return MapToResponse(product);
        }

        public async Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request)
        {
            // Valida duplicado por nombre + categoría.
            // Si ya existe, corresponde PRD-003.
            var existingProduct = await _repository.GetByNameAndCategoryAsync(
                request.Nombre,
                request.Categoria);

            if (existingProduct is not null)
                throw new BusinessRuleException(
                    ProductErrorCodes.DuplicateProduct,
                    $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

            // Crea la entidad de dominio.
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre.Trim(),
                Descripcion = request.Descripcion?.Trim(),
                Precio = request.Precio,
                Stock = request.Stock,
                Categoria = request.Categoria.Trim(),
                FechaCreacion = DateTime.UtcNow
            };

            var createdProduct = await _repository.CreateAsync(product);

            _logger.LogInformation(
                "Producto creado correctamente. ProductId: {ProductId}",
                createdProduct.Id);

            return MapToResponse(createdProduct);
        }

        public async Task<ProductResponseDto> UpdateAsync(Guid id, UpdateProductRequestDto request)
        {
            // Verifica si el producto existe.
            var product = await _repository.GetByIdAsync(id);

            if (product is null)
                throw new NotFoundException(
                    ProductErrorCodes.ProductNotFound,
                    "Producto no encontrado.");

            // Si se intenta actualizar a un nombre/categoría que ya usa otro producto,
            // se evita el duplicado.
            var existingProduct = await _repository.GetByNameAndCategoryAsync(
                request.Nombre,
                request.Categoria);

            if (existingProduct is not null && existingProduct.Id != id)
                throw new BusinessRuleException(
                    ProductErrorCodes.DuplicateProduct,
                    $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

            // Se actualizan solo los campos editables.
            product.Nombre = request.Nombre.Trim();
            product.Descripcion = request.Descripcion?.Trim();
            product.Precio = request.Precio;
            product.Stock = request.Stock;
            product.Categoria = request.Categoria.Trim();

            var updatedProduct = await _repository.UpdateAsync(product);

            _logger.LogInformation(
                "Producto actualizado correctamente. ProductId: {ProductId}",
                updatedProduct.Id);

            return MapToResponse(updatedProduct);
        }

        public async Task DeleteAsync(Guid id)
        {
            // Verifica primero que el producto exista.
            var product = await _repository.GetByIdAsync(id);

            if (product is null)
            {
                throw new NotFoundException(
                    ProductErrorCodes.ProductNotFound,
                    "Producto no encontrado.");
            }

            /*
             * Consulta Orders.API antes de eliminar.
             */
            var hasActiveOrders =
                await _ordersClient.HasActiveOrdersAsync(id);

            if (hasActiveOrders)
            {
                _logger.LogWarning(
                    "No se puede eliminar el producto {ProductId} porque tiene órdenes activas. ErrorCode: {ErrorCode}",
                    id,
                    ProductErrorCodes.ProductWithActiveOrders);

                throw new BusinessRuleException(
                    ProductErrorCodes.ProductWithActiveOrders,
                    "El producto tiene órdenes activas y no puede eliminarse.");
            }

            await _repository.DeleteAsync(id);

            _logger.LogInformation(
                "Producto eliminado correctamente. ProductId: {ProductId}",
                id);
        }

        private static ProductResponseDto MapToResponse(Product product)
        {
            // Mapea la entidad de dominio al DTO de salida.
            return new ProductResponseDto
            {
                Id = product.Id,
                Nombre = product.Nombre,
                Descripcion = product.Descripcion,
                Precio = product.Precio,
                Stock = product.Stock,
                Categoria = product.Categoria,
                FechaCreacion = product.FechaCreacion
            };
        }
    }
}
