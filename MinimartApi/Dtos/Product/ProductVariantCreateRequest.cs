using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Product {
    public class ProductVariantCreateRequest {
        [Required, MaxLength(255)]
        public string VariantName { get; internal set; }

        [Required, MaxLength(100)]
        public string SKU { get; internal set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; internal set; }

        //[FileExtensions(Extensions = "jpg,jpeg,png,gif,webp", ErrorMessage = "Invalid image format. Allowed formats are jpg, jpeg, png, gif.")]
        public IFormFile? Image { get; set; }
    }
}
