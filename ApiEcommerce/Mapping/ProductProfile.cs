using System;
using ApiEcommerce.Models.Dtos;
using AutoMapper;

namespace ApiEcommerce.Mapping;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Category, ProductDto>().ReverseMap();
        CreateMap<Category, CreateProductDto>().ReverseMap();
        CreateMap<Category, UpdateProductDto>().ReverseMap();
    }
}
