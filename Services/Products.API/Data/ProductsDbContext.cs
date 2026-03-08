using Microsoft.EntityFrameworkCore;

namespace Products.API.Data;

public class ProductsDbContext : DbContext
{
    public ProductsDbContext(DbContextOptions<ProductsDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Stock).IsRequired();
        });

        // Seed data
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop Dell XPS", Price = 1200m, Stock = 10, CreatedAt = DateTime.UtcNow },
            new Product { Id = 2, Name = "Mouse Logitech MX", Price = 50m, Stock = 100, CreatedAt = DateTime.UtcNow },
            new Product { Id = 3, Name = "Teclado Mecánico", Price = 120m, Stock = 50, CreatedAt = DateTime.UtcNow }
        );
    }
}
