using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Product
{
    public class ProductUpdateRequest
    {
        [Required, MaxLength(255)]
        public string? Name { get; set; }
        public string? Description { get; set; }

        public IFormFile? Image { get; set; }
    }
}
