using Microsoft.EntityFrameworkCore;
using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Electronics" },
            new Category { Id = 2, Name = "Accessories" });

        modelBuilder.Entity<Product>(b =>
        {
            b.Property(p => p.Price).HasPrecision(18, 2);
            b.HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId);
            b.HasData(
                new Product { Id = 1, Name = "Laptop", Price = 25000m, CategoryId = 1 },
                new Product { Id = 2, Name = "Phone", Price = 15000m, CategoryId = 1 },
                new Product { Id = 3, Name = "Mechanical Keyboard", Price = 2500m, CategoryId = 2 });
        });
    }
}
