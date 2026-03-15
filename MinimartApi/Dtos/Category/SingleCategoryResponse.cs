using System.Collections;

namespace MinimartApi.Dtos.Category
{
    public class SingleCategoryResponse
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public ArrayList Children { get; set; }
    }
}
