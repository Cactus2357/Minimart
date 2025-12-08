namespace MinimartApi.Models {
    public class PurchaseOrderItem {
        public int PurchaseOrderItemId { get; set; }
        public int PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal CostPrice { get; set; }
    }
}
