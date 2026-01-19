using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models {
    public class ProductVariant {
        [Key]
        public int VariantId { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required, MaxLength(100)]
        public string SKU { get; set; }

        [Required, MaxLength(255)]
        public string VariantName { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PriceHistory> PriceHistories { get; set; }

    }
}
