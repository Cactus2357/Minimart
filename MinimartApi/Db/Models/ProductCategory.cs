using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Db.Models {
    public class ProductCategory {
        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
