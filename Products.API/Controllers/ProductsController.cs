using Microsoft.AspNetCore.Mvc;
using ECommerce_G24.Products.API.Dtos;
using ECommerce_G24.Products.API.Services;

namespace ECommerce_G24.Products.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Obtiene la lista de productos.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? nombre,
            [FromQuery] string? categoria)
        {
            var products = await _productService.GetAllAsync(nombre, categoria);

            return Ok(products);
        }

        /// <summary>
        /// Obtiene un producto por ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _productService.GetByIDAsync(id);

            return Ok(product);
        }

        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create(
            [FromBody] CreateProductRequestDto request)
        {
            var product = await _productService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = product.Id },
                product);
        }

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateProductRequestDto request)
        {
            var product = await _productService.UpdateAsync(id, request);

            return Ok(product);
        }

        /// <summary>
        /// Elimina un producto.
        /// </summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _productService.DeleteAsync(id);

            return NoContent();
        }
    }
}