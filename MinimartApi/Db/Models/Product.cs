    using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models {
    public class Product {
        [Key]
        public int ProductId { get; set; }
        
        [Required, MaxLength(255)]
        public string Name { get; set; }
        
        public string Description { get; set; }

        [Url]
        public string? ImageUrl { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } // Pending, Approved, Rejected

        public bool IsDeleted { get; set; } = false;

        public Guid? CreatedBy { get; set; } // Staff
        public User? Creator { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<ProductVariant> Variants { get; set; }
        public ICollection<ProductCategory> ProductCategories { get; set; }
    }
}
