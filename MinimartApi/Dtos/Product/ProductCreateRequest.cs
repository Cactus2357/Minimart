using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Product {
    public class ProductCreateRequest {
        [Required, MaxLength(255)]
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
