using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using AutoMapper;

using ApiEcommerce.Constants;
using ApiEcommerce.Models.Dtos;
using ApiEcommerce.Repository.IRepository;
using Asp.Versioning;

namespace ApiEcommerce.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoriesController(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        public IActionResult GetCategories()
        {
            var categories = _categoryRepository.GetCategories();
            var categogiesDto = new List<CategoryDto>();

            foreach (var category in categories)
            {
                categogiesDto.Add(_mapper.Map<CategoryDto>(category));
            }
            return Ok(categogiesDto);
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
        [MapToApiVersion("2.0")]
        public IActionResult GetCategoriesOrderedById()
        {
            var categories = _categoryRepository.GetCategories().OrderBy(item => item.Id);
            var categogiesDto = new List<CategoryDto>();

            foreach (var category in categories)
            {
                categogiesDto.Add(_mapper.Map<CategoryDto>(category));
            }
            return Ok(categogiesDto);
        }
        
        [AllowAnonymous]
        [HttpGet("{id:int}", Name = "GetCategoryById")]
        [ResponseCache(CacheProfileName = CacheProfiles.Default10)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
        public IActionResult GetCategoryById(int id)
        {
            var category = _categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound($"No category {id} found");
            }
            var categoryDto = _mapper.Map<CategoryDto>(category);
            return Ok(categoryDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            if (createCategoryDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_categoryRepository.CategoryExistsByName(createCategoryDto.Name))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, $"Category {createCategoryDto.Name} already exists.");
                return BadRequest(ModelState);
            }
            var category = _mapper.Map<Category>(createCategoryDto);
            if (!_categoryRepository.CreateCategory(category))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, "Something went wrong while saving the category.");
                return StatusCode(500, ModelState);
            }
            return CreatedAtRoute("GetCategoryById", new { id = category.Id }, category);
        }

        [HttpPatch("{id:int}", Name = "UpdateCategoryById")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public IActionResult UpdateCategoryById(int id, [FromBody] CreateCategoryDto updateCategoryDto)
        {
            if (!_categoryRepository.CategoryExistsById(id))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, $"Category {id} doesn't exists.");
                return NotFound(ModelState);
            }
            if (updateCategoryDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_categoryRepository.CategoryExistsByName(updateCategoryDto.Name))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, $"Category {updateCategoryDto.Name} already exists.");
                return BadRequest(ModelState);
            }
            var category = _mapper.Map<Category>(updateCategoryDto);
            category.Id = id;
            if (!_categoryRepository.UpdateCategory(category))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, "Something went wrong while updating the category.");
                return StatusCode(500, ModelState);
            }
            return Ok($"Category {id} have been updated");
        }

        [HttpDelete("{id:int}", Name = "DeleteCategoryById")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status204NoContent)]
        public IActionResult DeleteCategoryById(int id)
        {
            if (!_categoryRepository.CategoryExistsById(id))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, $"Category {id} doesn't exists.");
                return NotFound(ModelState);
            }
            var category = _categoryRepository.GetCategoryById(id);
            if (category == null)
            {
                return NotFound($"Category {id} doesn't exists.");
            }
            if (!_categoryRepository.DeleteCategory(category))
            {
                ModelState.AddModelError(Constants.Constants.CustomErrorKey, "Something went wrong while deleting the category.");
                return StatusCode(500, ModelState);
            }
            return Ok($"Category {id} have been deleted");
        }
    }
}
