using backenddef.Models;
using Microsoft.EntityFrameworkCore;

namespace backenddef.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Review> Reviews => Set<Review>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración Products -> User
            modelBuilder.Entity<Product>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración Orders -> Cliente y Empresa
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Cliente)
                .WithMany()
                .HasForeignKey(o => o.ClienteId)
                .OnDelete(DeleteBehavior.NoAction); // Evita cascada múltiple

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Empresa)
                .WithMany()
                .HasForeignKey(o => o.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración OrderItems -> Order y Product
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configuración Reviews -> Product y Cliente
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Cliente)
                .WithMany()
                .HasForeignKey(r => r.ClienteId)
                .OnDelete(DeleteBehavior.Restrict); // Evita múltiple cascade path
        }
    }
}
