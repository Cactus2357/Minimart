namespace MinimartApi.Dtos.Product {
    public class ProductResponse {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string>? Categories { get; set; }
        //p.ProductId,
        //p.Name,
        //p.Description,
        //p.Price

    }
}
