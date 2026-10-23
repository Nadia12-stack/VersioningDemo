using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Application.Products;

/// <summary>
/// Shared by ALL API versions. Versions differ only in the Contract (DTOs),
/// never in the business logic.
/// </summary>
public interface IProductService
{
    // ── V1 (unchanged) ──────────────────────────────────────────────────────
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

    // ── V2: paginated read, update with concurrency, soft delete ────────────
    Task<PagedResult<ProductDto>> GetAsync(
        int pageNumber, int pageSize, string? search, CancellationToken ct);

    Task<UpdateResult> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct);

    Task<bool> DeleteAsync(int id, CancellationToken ct);
}
