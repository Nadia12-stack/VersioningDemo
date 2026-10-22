using Microsoft.EntityFrameworkCore;
using VersioningDemo.Application.Products;
using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Infrastructure.Persistence;

public sealed class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
        => await db.Products.AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => db.Products.AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}
