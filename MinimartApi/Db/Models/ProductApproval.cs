using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models
{
    public class ProductApproval
    {
        [Key]
        public int ApprovalId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required, MaxLength(50)]
        public string Action { get; set; } // Create, Update

        [Required, MaxLength(50)]
        public string Status { get; set; } // Pending, Approved, Rejected

        public Guid? ApprovedBy { get; set; } // Staff who approved/rejected

        public DateTime? ApprovedAt { get; set; }
    }
}
