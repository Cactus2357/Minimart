namespace MinimartApi.Models {
    public class Customer {
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int LoyaltyPoints { get; set; }

        public virtual ICollection<Sale> Sales { get; set; }
    }
}
