using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Product
{
    public class ProductVariantUpdateRequest
    {
        [Required, MaxLength(255)]
        public string? VariantName { get; set; }

        public IFormFile? Image { get; set; }

        [Required, MaxLength(100)]
        public string? SKU { get; set; }

        [Range(0, int.MaxValue)]
        public int? StockQuantity { get; set; }
    }
}
