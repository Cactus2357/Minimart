using AutoMapper;
using MinimartApi.Db.Models;
using MinimartApi.Dtos.Category;
using MinimartApi.Dtos.Product;

namespace MinimartApi.Mappers
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<Category, CategoryResponse>().ReverseMap();
            object value = CreateMap<Product, ProductResponse>().ForMember(
                dest => dest.Categories,
                opt => opt.MapFrom(
                    src => src.ProductCategories != null
                        ? src.ProductCategories
                            .Where(pc => pc.Category != null && !pc.Category.IsDeleted)
                            .Select(pc => pc.Category!.Name)
                            .ToList()
                        : null
                )
            ).ReverseMap();
        }
    }
}
