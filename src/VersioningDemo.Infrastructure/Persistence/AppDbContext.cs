using Microsoft.EntityFrameworkCore;
using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up all IEntityTypeConfiguration<T> classes in this assembly (e.g. ProductConfiguration)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Accessories" });

        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop",             Price = 25000m, CategoryId = 1 },
            new Product { Id = 2, Name = "Phone",              Price = 15000m, CategoryId = 1 },
            new Product { Id = 3, Name = "Mechanical Keyboard", Price = 2500m,  CategoryId = 2 });
    }
}
