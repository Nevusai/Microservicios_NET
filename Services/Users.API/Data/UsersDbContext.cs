using Microsoft.EntityFrameworkCore;

namespace Users.API.Data;

public class UsersDbContext : DbContext
{
    public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });

        // Seed data
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Email = "juan@example.com", Name = "Juan Perez", PasswordHash = "hash123", CreatedAt = DateTime.UtcNow },
            new User { Id = 2, Email = "maria@example.com", Name = "Maria Garcia", PasswordHash = "hash456", CreatedAt = DateTime.UtcNow }
        );
    }
}
