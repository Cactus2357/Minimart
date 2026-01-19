using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models {
    public class Address {
        [Key]
        public int AddressId { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public User User { get; set; }

        [Required, MaxLength(255)]
        public string ReceiverName { get; set; }

        [Required, Phone, MaxLength(50)]
        public string Phone { get; set; }

        [Required, MaxLength(500)]
        public string AddressLine { get; set; }

        public bool IsDefault { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
