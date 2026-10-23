using Microsoft.EntityFrameworkCore;
using VersioningDemo.Application.Products;
using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Infrastructure.Persistence;

public sealed class ProductRepository(AppDbContext db) : IProductRepository
{
    // ── V1 ──────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
        => await db.Products.AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => db.Products.AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    // ── V2 ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// AsNoTracking + Select → single JOIN query, no N+1.
    /// The Name.StartsWith(search) call uses the index on Name (index seek, not scan).
    /// Use query.ToQueryString() in development to inspect the generated SQL.
    /// </summary>
    public async Task<(IReadOnlyList<ProductDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? search, CancellationToken ct)
    {
        var query = db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.StartsWith(search)); // exploits the Name index

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            // Select projects to the DTO in one query — avoids loading entire Entity graph.
            // EF translates p.Category.Name to a JOIN, so no N+1 problem.
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Category.Name, p.RowVersion))
            .ToListAsync(ct);

        return (items, total);
    }

    /// <summary>
    /// Loads the tracked entity then assigns only the allowed fields.
    /// We deliberately do NOT call db.Update(product) — Change Tracking detects
    /// the mutations automatically and generates a minimal UPDATE statement.
    /// Setting RowVersion OriginalValue lets EF append WHERE RowVersion = @original
    /// so the DB raises an optimistic-concurrency violation on stale writes.
    /// </summary>
    public async Task<UpdateResult> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null) return UpdateResult.NotFound;

        product.Name  = dto.Name;
        product.Price = dto.Price;

        // Tell EF what version the client saw — it includes this in the WHERE clause.
        db.Entry(product).Property(p => p.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            await db.SaveChangesAsync(ct);
            return UpdateResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return UpdateResult.Conflict;
        }
    }

    /// <summary>
    /// Soft delete: sets IsDeleted = true.
    /// The HasQueryFilter(p => !p.IsDeleted) on ProductConfiguration hides this
    /// row from all future queries automatically.
    /// To query deleted rows explicitly, call IgnoreQueryFilters().
    /// </summary>
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null) return false;

        product.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return true;
    }
}
