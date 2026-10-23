using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Application.Products;

public sealed class ProductService(IProductRepository repository) : IProductService
{
    // ── V1 ──────────────────────────────────────────────────────────────────
    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
        => repository.GetAllAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => repository.GetByIdAsync(id, cancellationToken);

    // ── V2 ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// AsNoTracking + Select projection → single JOIN query, no N+1.
    /// pageSize is clamped to [1..100]; pageNumber floors at 1.
    /// </summary>
    public async Task<PagedResult<ProductDto>> GetAsync(
        int pageNumber, int pageSize, string? search, CancellationToken ct)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize   = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await repository.GetPagedAsync(pageNumber, pageSize, search, ct);

        return new PagedResult<ProductDto>(items, pageNumber, pageSize, total);
    }

    /// <summary>
    /// Loads the tracked entity (no Update() call) and sets RowVersion OriginalValue
    /// so EF detects concurrent modifications and raises DbUpdateConcurrencyException.
    /// </summary>
    public Task<UpdateResult> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct)
        => repository.UpdateAsync(id, dto, ct);

    /// <summary>Sets IsDeleted = true — never physically removes the row.</summary>
    public Task<bool> DeleteAsync(int id, CancellationToken ct)
        => repository.DeleteAsync(id, ct);
}
