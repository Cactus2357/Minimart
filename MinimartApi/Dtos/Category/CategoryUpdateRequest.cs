using System.ComponentModel.DataAnnotations;

namespace MinimartApi.Dtos.Category {
    public class CategoryUpdateRequest {
        [Required, StringLength(100)]
        public string Name { get; set; }

        public int? ParentCategoryId { get; set; }
    }
}
