using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models
{
    public class PriceHistory
    {
        [Key]
        public int PriceHistoryId { get; set; }

        [Required]
        public int VariantId { get; set; }
        public ProductVariant ProductVariant { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OriginalPrice { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? SalePrice { get; set; }

        [Range(0, 100)]
        public int DiscountPercent { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public Guid? CreatedBy { get; set; }
        public User? Creator { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
