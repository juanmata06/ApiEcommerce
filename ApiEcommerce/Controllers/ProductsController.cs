using ApiEcommerce.Models;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ApiEcommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "admin")]
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

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
        public IActionResult GetProducts()
        {
            var items = _productRepository.GetProducts();
            var itemsDto = _mapper.Map<List<ProductDto>>(items);
            return Ok(itemsDto);
        }

        [AllowAnonymous]
        [HttpGet("{id:int}", Name = "GetProductById")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
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
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            if (createProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_productRepository.ProductExistsByName(createProductDto.Name))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, $"Product {createProductDto.Name} already exists.");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.CategoryExistsById(createProductDto.CategoryId))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, $"Category {createProductDto.CategoryId} doesn't exists.");
                return BadRequest(ModelState);
            }
            var product = _mapper.Map<Product>(createProductDto);
            if (!_productRepository.CreateProduct(product))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, "Something went wrong while creating the product.");
                return StatusCode(500, ModelState);
            }
            var createdProduct = _productRepository.GetProductById(product.Id);
            var productDto = _mapper.Map<ProductDto>(createdProduct);
            return CreatedAtRoute("GetProductById", new { id = product.Id }, productDto);
        }

        [AllowAnonymous]
        [HttpGet("search-by-category-id/{id:int}", Name = "GetProductsByCategoryId")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
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

        [AllowAnonymous]
        [HttpGet("search-by-name/{name}", Name = "GetProductsByName")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
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

        [HttpPatch("buy-product/{name}/{quantity:int}", Name = "BuyProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
        public IActionResult BuyProduct(string name, int quantity)
        {
            if (string.IsNullOrEmpty(name) || quantity <= 0)
            {
                return BadRequest($"Product's name or quantity aren't valid");
            }
            var foundProduct = _productRepository.ProductExistsByName(name);
            if (!foundProduct)
            {
                return NotFound($"No product found with that name");
            }
            if (!_productRepository.BuyProduct(name, quantity))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, $"The requested quantity is greater than the available stock");
                return BadRequest(ModelState);
            }
            return Ok($"{quantity} {(quantity == 1 ? "unit" : "units")} of the product {name} have been purchased");
        }

        [HttpPut("{productId}:int", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult UpdateProduct(int productId, [FromBody] UpdateProductDto updateProductDto)
        {
            if (updateProductDto == null)
            {
                return BadRequest(ModelState);
            }
            if (!_productRepository.ProductExistsById(productId))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, $"Product {productId} doesn't exists.");
                return BadRequest(ModelState);
            }
            if (_productRepository.ProductExistsByName(updateProductDto.Name))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, $"Product {updateProductDto.Name} already exists.");
                return BadRequest(ModelState);
            }
            if (!_categoryRepository.CategoryExistsById(updateProductDto.CategoryId))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, $"Category {updateProductDto.CategoryId} doesn't exists.");
                return BadRequest(ModelState);
            }
            var product = _mapper.Map<Product>(updateProductDto);
            product.Id = productId;
            if (!_productRepository.UpdateProduct(product))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, "Something went wrong while updating the product.");
                return StatusCode(500, ModelState);
            }
            return Ok($"Product {productId} have been updated");
        }

        [HttpDelete("{id:int}", Name = "DeleteProductById")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult DeleteProductById(int id)
        {
            if(id == 0)
            {
                return BadRequest(ModelState);
            }
            var item = _productRepository.GetProductById(id);
            if (item == null)
            {
                return NotFound($"No product {id} found");
            }
            if (!_productRepository.DeleteProduct(item))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, "Something went wrong while deleting the product.");
                return StatusCode(500, ModelState);
            }
            return Ok($"Product {id} have been deleted");
        }

    }
}
