using Microsoft.EntityFrameworkCore;

namespace MinimartApi.Db.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<PriceHistory> PriceHistories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(e =>
            {
                e.HasIndex(u => u.Email).IsUnique();
                e.HasIndex(u => u.Username).IsUnique();
                e.HasQueryFilter(u => !u.IsDeleted);
            });

            builder.Entity<Role>(e =>
            {
                e.HasIndex(r => r.Name).IsUnique();
            });

            builder.Entity<UserRole>(e =>
            {
                e.HasKey(ur => new { ur.UserId, ur.RoleId });
                e.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Address>(e =>
            {
                e.HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            });

            builder.Entity<Category>(e =>
            {
                e.HasOne(c => c.ParentCategory)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
                e.HasQueryFilter(c => !c.IsDeleted);
                e.HasOne(o => o.Creator)
                .WithMany()
                .HasForeignKey(o => o.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Product>(e =>
            {
                e.HasQueryFilter(p => !p.IsDeleted);
                e.Property(p => p.Status)
                .HasMaxLength(50)
                .IsRequired();
                e.HasOne(o => o.Creator)
                .WithMany()
                .HasForeignKey(o => o.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ProductCategory>(e =>
            {
                e.HasKey(pc => new { pc.ProductId, pc.CategoryId });
                e.HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<ProductVariant>(e =>
            {
                e.HasIndex(v => v.SKU).IsUnique();
                e.Property(v => v.Stock).HasDefaultValue(0);
                e.HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<PriceHistory>(e =>
            {
                e.Property(p => p.OriginalPrice).HasPrecision(18, 2);
                e.Property(p => p.SalePrice).HasPrecision(18, 2);
                e.HasOne(ph => ph.ProductVariant)
                .WithMany(v => v.PriceHistories)
                .HasForeignKey(ph => ph.VariantId)
                .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(ph => ph.Creator)
                .WithMany()
                .HasForeignKey(ph => ph.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(p => new { p.VariantId, p.EffectiveFrom, p.EffectiveTo });
            });

            builder.Entity<Order>(e =>
            {
                e.Property(o => o.CurrentStatus)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

                e.Property(o => o.TotalAmount).HasPrecision(18, 2);
                e.HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OrderItem>(e =>
            {
                e.Property(o => o.UnitPrice).HasPrecision(18, 2);
                e.HasIndex(oi => new { oi.OrderId, oi.VariantId });
                e.HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(oi => oi.ProductVariant)
                .WithMany()
                .HasForeignKey(oi => oi.VariantId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<OrderStatusHistory>(e =>
            {
                e.Property(h => h.OldStatus)
                .HasConversion<string>()
                .HasMaxLength(50);
                e.Property(h => h.NewStatus)
                .HasConversion<string>()
                .HasMaxLength(50);
                e.HasOne(h => h.Order)
                .WithMany()
                .HasForeignKey(h => h.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                e.HasOne(h => h.Changer)
                .WithMany()
                .HasForeignKey(h => h.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Payment>(e =>
            {
                e.HasIndex(p => p.OrderId).IsUnique();
                e.Property(p => p.Amount).HasPrecision(18, 2);
                e.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();
                e.HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
            });
        }

    }
}
