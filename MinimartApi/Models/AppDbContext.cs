using Microsoft.EntityFrameworkCore;

namespace MinimartApi.Models {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Stock> Stocks { get; set; }
        public DbSet<StockTransaction> StockTransactions { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasKey(u => u.UserId);

            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<User>()
                .HasMany(u => u.Sales)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Customer>()
                .HasMany(c => c.Sales)
                .WithOne(s => s.Customer)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);


            builder.Entity<Stock>()
                .HasKey(s => s.ProductId);

            builder.Entity<Stock>()
                .HasOne(s => s.Product)
                .WithOne(p => p.Stock)
                .HasForeignKey<Stock>(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<StockTransaction>()
                .HasOne(t => t.Product)
                .WithMany()
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<Supplier>()
                .HasMany(s => s.PurchaseOrders)
                .WithOne(p => p.Supplier)
                .HasForeignKey(p => p.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<PurchaseOrderItem>()
                .HasOne(i => i.PurchaseOrder)
                .WithMany(p => p.Items)
                .HasForeignKey(i => i.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PurchaseOrderItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.PurchaseOrderItems)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<SaleItem>()
                .HasOne(i => i.Sale)
                .WithMany(s => s.Items)
                .HasForeignKey(i => i.SaleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SaleItem>()
                .HasOne(i => i.Product)
                .WithMany(p => p.SaleItems)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            builder.Entity<Product>().Property(p => p.CostPrice).HasPrecision(18, 2);

            builder.Entity<PurchaseOrder>().Property(p => p.TotalAmount).HasPrecision(18, 2);
            builder.Entity<PurchaseOrderItem>().Property(p => p.CostPrice).HasPrecision(18, 2);

            builder.Entity<Sale>().Property(s => s.TotalAmount).HasPrecision(18, 2);
            builder.Entity<SaleItem>().Property(s => s.Price).HasPrecision(18, 2);
        }
    }
}
