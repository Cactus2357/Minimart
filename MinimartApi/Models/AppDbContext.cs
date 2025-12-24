using Microsoft.EntityFrameworkCore;

namespace MinimartApi.Models {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
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

            builder.Entity<User>(e => {
                e.HasIndex(u => u.Email).IsUnique().HasFilter("[IsDeleted] = 0");
                e.Property(u => u.Email).IsRequired().HasMaxLength(256);
                e.HasIndex(u => u.Username).IsUnique().HasFilter("[IsDeleted] = 0");
                e.Property(u => u.Username).IsRequired().HasMaxLength(100);
                e.Property(u => u.PasswordHash).IsRequired();
                e.Property(u => u.IsEmailConfirmed).HasDefaultValue(false);
                e.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                e.HasMany(u => u.Sales).WithOne(s => s.User).HasForeignKey(s => s.UserId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
                e.HasQueryFilter(u => !u.IsDeleted);
            });


            builder.Entity<Role>(e => {
                e.HasIndex(r => r.Name).IsUnique().HasFilter("[IsDeleted] = 0");
                e.Property(r => r.Name).IsRequired().HasMaxLength(50);

                e.HasQueryFilter(u => !u.IsDeleted);
            });


            builder.Entity<UserRole>(e => {
                e.HasKey(ur => new { ur.UserId, ur.RoleId });

                e.HasOne(ur => ur.User).WithMany(u => u.UserRoles).HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            });


            builder.Entity<Category>(e => {
                e.Property(c => c.Name).IsRequired().HasMaxLength(100);
                e.HasIndex(c => c.Name).IsUnique().HasFilter("[IsDeleted] = 0");

                e.HasMany(c => c.Products).WithOne(p => p.Category).HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
                e.HasQueryFilter(u => !u.IsDeleted);
            });


            builder.Entity<Customer>(e => {
                e.Property(c => c.Name).IsRequired().HasMaxLength(150);
                e.Property(c => c.Phone).HasMaxLength(20);
                e.HasIndex(c => c.Phone).IsUnique().HasFilter("[Phone] IS NOT NULL AND [IsDeleted] = 0");

                e.HasMany(c => c.Sales).WithOne(s => s.Customer).HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Restrict);
                e.HasQueryFilter(u => !u.IsDeleted);
            });


            builder.Entity<Product>(e => {
                e.Property(p => p.Name).IsRequired().HasMaxLength(200);
                e.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
                e.Property(p => p.CostPrice).HasPrecision(18, 2);
                e.HasIndex(p => p.Name);

                e.HasMany(p => p.PurchaseOrderItems).WithOne(i => i.Product).HasForeignKey(i => i.ProductId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
                e.HasMany(p => p.SaleItems).WithOne(i => i.Product).HasForeignKey(i => i.ProductId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
                e.HasQueryFilter(u => !u.IsDeleted);
            });


            builder.Entity<Stock>(e => {
                e.HasKey(s => s.ProductId);
                e.Property(s => s.Quantity).IsRequired();

                e.HasOne(s => s.Product).WithOne(p => p.Stock).HasForeignKey<Stock>(s => s.ProductId).OnDelete(DeleteBehavior.Restrict);
            });


            builder.Entity<StockTransaction>(e => {
                e.Property(t => t.Quantity).IsRequired();
                e.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                e.HasOne(t => t.Product).WithMany().HasForeignKey(t => t.ProductId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
            });


            builder.Entity<Supplier>(e => {
                e.Property(s => s.Name).IsRequired().HasMaxLength(200);
                e.Property(s => s.Email).HasMaxLength(256);
                e.HasIndex(s => s.Email).HasFilter("[Email] IS NOT NULL AND [IsDeleted] = 0");

                e.HasMany(s => s.PurchaseOrders).WithOne(po => po.Supplier).HasForeignKey(po => po.SupplierId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
                e.HasQueryFilter(u => !u.IsDeleted);
            });


            builder.Entity<PurchaseOrder>(e => {
                e.Property(p => p.TotalAmount).HasPrecision(18, 2).IsRequired();
                e.Property(p => p.OrderDate).HasDefaultValueSql("GETUTCDATE()");

                e.HasMany(po => po.Items).WithOne(i => i.PurchaseOrder).HasForeignKey(i => i.PurchaseOrderId).OnDelete(DeleteBehavior.Restrict);
            });


            builder.Entity<PurchaseOrderItem>(e => {
                e.Property(i => i.Quantity).IsRequired();
                e.Property(i => i.CostPrice).HasPrecision(18, 2).IsRequired();
            });


            builder.Entity<Sale>(e => {
                e.Property(s => s.TotalAmount).HasPrecision(18, 2).IsRequired();
                e.Property(s => s.SaleDate).HasDefaultValueSql("GETUTCDATE()");

                e.HasMany(s => s.Items).WithOne(si => si.Sale).HasForeignKey(si => si.SaleId).OnDelete(DeleteBehavior.Restrict);
            });


            builder.Entity<SaleItem>(e => {
                e.Property(i => i.Quantity).IsRequired();
                e.Property(i => i.Price).HasPrecision(18, 2).IsRequired();
            });
        }

        private void ApplySoftDelete() {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries()) {
                var isDeletedProp = entry.Metadata.FindProperty("IsDeleted");
                if (isDeletedProp == null) continue;

                var deletedAtProp = entry.Metadata.FindProperty("DeletedAt");
                var isDeletedName = isDeletedProp.Name;
                var deletedAtName = deletedAtProp?.Name;

                if (entry.State == EntityState.Deleted) {
                    entry.State = EntityState.Modified;

                    var isDeleted = entry.Property(isDeletedName);
                    isDeleted.CurrentValue = true;
                    isDeleted.IsModified = true;

                    if (deletedAtName != null) {
                        var deletedAt = entry.Property(deletedAtName);
                        if (deletedAt.CurrentValue == null) {
                            deletedAt.CurrentValue = now;
                            deletedAt.IsModified = true;
                        }
                    }

                    continue;
                }

                if (entry.State == EntityState.Added && deletedAtName != null) {
                    var isDeleted = (bool?)entry.Property(isDeletedName).CurrentValue;
                    if (isDeleted == true) {
                        var deletedAt = entry.Property(deletedAtName);
                        if (deletedAt.CurrentValue == null) {
                            deletedAt.CurrentValue = now;
                            deletedAt.IsModified = true;
                        }
                    }

                    continue;
                }

                if (entry.State == EntityState.Modified && deletedAtName != null) {
                    var isDeletedPropEntry = entry.Property(isDeletedName);
                    var isDeleted = (bool?)isDeletedPropEntry.CurrentValue;

                    if (isDeleted == true && entry.Property(deletedAtName).CurrentValue == null) {
                        entry.Property(deletedAtName).CurrentValue = now;
                        entry.Property(deletedAtName).IsModified = true;
                        isDeletedPropEntry.IsModified = true;
                    }
                    else if (isDeleted == false && entry.Property(deletedAtName).CurrentValue != null) {
                        entry.Property(deletedAtName).CurrentValue = null;
                        entry.Property(deletedAtName).IsModified = true;
                        isDeletedPropEntry.IsModified = true;
                    }
                }
            }
        }

        public override int SaveChanges() {
            ApplySoftDelete();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
            ApplySoftDelete();
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
