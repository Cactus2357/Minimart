using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models
{
    public class OrderApproval
    {
        [Key]
        public int ApprovalId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required, MaxLength(50)]
        public string Action { get; set; } // Cancel, Refund

        [Required, MaxLength(50)]
        public string Status { get; set; } // Pending, Approved, Rejected

        public Guid? ApprovedBy { get; set; } // Staff who approved/rejected

        public DateTime? ApprovedAt { get; set; }

    }
}
