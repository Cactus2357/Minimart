using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Order
{
    public class OrderItemRequest
    {
        [Required]
        public int VariantId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
