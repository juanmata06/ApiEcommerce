using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var items = _productRepository.GetProducts();
            var itemsDto = _mapper.Map<List<ProductDto>>(items);
            return Ok(itemsDto);
        }

        [HttpGet("{id:int}", Name = "GetProductById")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductById(int id)
        {
            var item = _productRepository.GetProductById(id);
            if (item == null)
            {
                return NotFound($"No product {id} found");
            }
            var itemDto = _mapper.Map<ProductDto>(item);
            return Ok(itemDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (createProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_productRepository.ProductExistsByName(createProductDto.Name))
            {
                ModelState.AddModelError("CustomError", $"Product {createProductDto.Name} already exists.");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.CategoryExistsById(createProductDto.CategoryId))
            {
                ModelState.AddModelError("CustomError", $"Category {createProductDto.CategoryId} doesn't exists.");
                return BadRequest(ModelState);
            }
            var product = _mapper.Map<Product>(createProductDto);
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError("CustomError", "Something went wrong while creating the product.");
                return StatusCode(500, ModelState);
            }
            var createdProduct = _productRepository.GetProductById(product.Id);
            var productDto = _mapper.Map<ProductDto>(createdProduct);
            return CreatedAtRoute("GetProductById", new { id = product.Id }, productDto);
        }

        [HttpGet("search-by-category-id/{id:int}", Name = "GetProductsByCategoryId")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsByCategoryId(int id)
        {
            var items = _productRepository.GetProductsByCategoryId(id);
            if (items.Count == 0)
            {
                return NotFound($"No products in category {id} found");
            }
            var itemsDto = _mapper.Map<List<ProductDto>>(items);
            return Ok(itemsDto);
        }

        [HttpGet("search-by-name/{name}", Name = "GetProductsByName")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProductsByName(string name)
        {
            var items = _productRepository.GetProductsByName(name);
            if (items.Count == 0)
            {
                return NotFound($"No products {name} found");
            }
            var itemsDto = _mapper.Map<List<ProductDto>>(items);
            return Ok(itemsDto);
        }
    }
}
