using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(p => p.Price)
               .HasPrecision(18, 2);

        // Index on Name: frequently searched/filtered column.
        // Converts table scans → index seeks. Trade-off: extra storage + slightly
        // slower INSERT/UPDATE, worth it only for high-cardinality query columns.
        builder.HasIndex(p => p.Name);

        // Soft Delete: EF automatically appends WHERE IsDeleted = 0 to every query.
        // Use IgnoreQueryFilters() when you explicitly need to see deleted records.
        builder.HasQueryFilter(p => !p.IsDeleted);

        // Concurrency token — mapped via [Timestamp] attribute on the entity.
        // On SQL Server this becomes a rowversion column (auto-incremented by DB).
        // On InMemory the attribute is sufficient; IsRowVersion() is SQL Server-specific.
        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryId);
    }
}
