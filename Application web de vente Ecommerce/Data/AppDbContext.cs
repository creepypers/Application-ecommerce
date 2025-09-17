using Microsoft.EntityFrameworkCore;
using TPECOM.Models;

namespace TPECOM.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Invoice> Invoices { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuration des relations
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Invoice)
                .WithOne(i => i.Order)
                .HasForeignKey<Invoice>(i => i.OrderId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Orders)
                .WithOne()
                .HasForeignKey(o => o.UserId);

            // Configuration de la propriété de navigation pour les produits vendus
            modelBuilder.Entity<OrderItem>()
                .HasOne<User>()
                .WithMany(u => u.SoldItems)
                .HasForeignKey(i => i.SellerId)
                .OnDelete(DeleteBehavior.NoAction);

            // Add decimal precision configuration to avoid the warnings
            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingCost)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Order>()
                .Property(o => o.Tax)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Price)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);
        }
    }
} 