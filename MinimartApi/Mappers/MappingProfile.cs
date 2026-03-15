using AutoMapper;
using MinimartApi.Db.Models;
using MinimartApi.Dtos.Category;
using MinimartApi.Dtos.Product;

namespace MinimartApi.Mappers
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Category, CategoryResponse>().ReverseMap();
            CreateMap<Product, ProductResponse>().ReverseMap();
        }
    }
}
