using System;
using System.ComponentModel.DataAnnotations;

namespace ApiEcommerce.Models.Dtos;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [MaxLength(50, ErrorMessage = "Category name max length is 50")]
    [MinLength(3, ErrorMessage = "Category name min length is 3")]
    public string Name { get; set; } = string.Empty;
}
