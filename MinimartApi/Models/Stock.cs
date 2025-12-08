namespace MinimartApi.Models {
    public class Stock {
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }
        public int Quantity { get; set; }
    }
}
