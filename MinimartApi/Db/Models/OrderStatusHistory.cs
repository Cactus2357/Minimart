using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models
{
    public class OrderStatusHistory
    {
        [Key]
        public int HistoryId { get; set; }

        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }

        [Required, MaxLength(50)]
        public string OldStatus { get; set; }

        [Required, MaxLength(50)]
        public string NewStatus { get; set; }

        public Guid? ChangedBy { get; set; }
        public User? Changer { get; set; }

        [Required]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
