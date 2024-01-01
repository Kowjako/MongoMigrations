using AutoMapper;
using MediatR.Application.CQRS.Products.Dto;
using MediatR.Application.CQRS.Products.QueryHandlers;

namespace MediatR.Application.MappingProfiles
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, ProductDto>();
        }
    }
}
