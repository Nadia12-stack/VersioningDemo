using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Application.Products;

public interface IProductRepository
{
    // ── V1 ──────────────────────────────────────────────────────────────────
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

    // ── V2 ──────────────────────────────────────────────────────────────────
    Task<(IReadOnlyList<ProductDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? search, CancellationToken ct);

    /// <summary>
    /// Loads the tracked entity, applies changes and saves.
    /// Sets OriginalValue of RowVersion so EF raises DbUpdateConcurrencyException on conflict.
    /// </summary>
    Task<UpdateResult> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct);

    /// <summary>Soft-deletes by setting IsDeleted = true.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
