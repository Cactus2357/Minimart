using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Product {
    public class ProductVariantCreateRequest {
        [Required, MaxLength(255)]
        public string VariantName { get; internal set; }

        [Required, MaxLength(100)]
        public string SKU { get; internal set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; internal set; }
    }
}
