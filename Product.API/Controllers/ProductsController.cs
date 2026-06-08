using Microsoft.AspNetCore.Mvc;
using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Services;

namespace ECommerce_G24.Products.API.Controllers
{
    // Controlador REST para administrar productos.
    [ApiController]
    [Route("api/products")]
    [Produces("application/json")]
    [Tags("Products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        // Constructor con inyección del servicio de productos.
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET /api/products
        // Lista productos con filtros opcionales por nombre y categoría.
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll(
            [FromQuery] string? nombre,
            [FromQuery] string? categoria)
        {
            // El servicio aplica los filtros opcionales.
            var products = await _productService.GetAllAsync(nombre, categoria);
            return Ok(products);
        }

        // GET /api/products/{id}
        // Obtiene un producto por ID.
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductResponseDto>> GetById(Guid id)
        {
            // Si el producto no existe, el servicio lanza NotFoundException.
            var product = await _productService.GetByIdAsync(id);
            return Ok(product);
        }

        // POST /api/products
        // Crea un producto nuevo.
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductResponseDto>> Create(CreateProductRequestDto request)
        {
            // Si hay duplicado por nombre y categoría, el servicio lanza BusinessRuleException PRD-003.
            var createdProduct = await _productService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdProduct.Id },
                createdProduct);
        }

        // PUT /api/products/{id}
        // Actualiza un producto existente.
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductResponseDto>> Update(
        Guid id,
        UpdateProductRequestDto request)
        {
            // Si el producto no existe, el servicio lanza NotFoundException PRD-001.
            var updatedProduct = await _productService.UpdateAsync(id, request);

            return Ok(updatedProduct);
        }

        // DELETE /api/products/{id}
        // Elimina un producto existente.
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            // Si tiene órdenes activas, el servicio lanza BusinessRuleException PRD-004.
            await _productService.DeleteAsync(id);
            return NoContent();
        }
    }
}