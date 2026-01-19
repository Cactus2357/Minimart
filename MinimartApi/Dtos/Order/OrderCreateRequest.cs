using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Order
{
    public class OrderCreateRequest
    {
        [Required]
        public int AddressId { get; set; }

        [Required]
        public List<OrderItemRequest> Items { get; set; }
    }
}
