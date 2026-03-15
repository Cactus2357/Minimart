using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Product
{
    public class PriceRequest
    {
        [Range(0, double.MaxValue)]
        public decimal? SalePrice { get; internal set; }

        [Range(0, 100)]
        public int DiscountPercent { get; internal set; }

        [Required]
        public DateTime EffectiveFrom { get; internal set; }

        public DateTime? EffectiveTo { get; internal set; }
    }
}
