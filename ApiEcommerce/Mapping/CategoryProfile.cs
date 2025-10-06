using System;
using ApiEcommerce.Models.Dtos;
using AutoMapper;

namespace ApiEcommerce.Mapping;

/*
    AutoMapper library simplifies object-to-object mapping,
    removing the need for manual property assignments. 
    A Profile in AutoMapper is a configuration class that defines
    the mapping rules between domain models (e.g., Category) and DTOs.
    It works as a "translator" that tells AutoMapper how to 
    convert one object type into another, making the code cleaner,
    easier to maintain, and less error-prone.
*/
public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Category, CreateCategoryDto>().ReverseMap();
    }
}
