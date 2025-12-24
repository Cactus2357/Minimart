using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos {
    public class CreateCategoryRequest {
        [Required, StringLength(100)]
        public string Name { get; set; }
        [StringLength(250)]
        public string Description { get; set; }
    }
}
