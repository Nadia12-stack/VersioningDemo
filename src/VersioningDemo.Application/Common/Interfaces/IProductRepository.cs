using VersioningDemo.Application.DTOs;
using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Application.Common.Interfaces;

/// <summary>
/// Product-specific repository contract.
/// Extends IRepository&lt;Product&gt; (LSP: a ProductRepository IS-A Repository&lt;Product&gt;)
/// and adds only the product-specific queries that cannot be expressed generically.
/// (ISP: callers that only need generic CRUD depend on IRepository&lt;Product&gt; directly.)
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    // ── V1 ───────────────────────────────────────────────────────────────────
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);

    // ── V2: paged read ────────────────────────────────────────────────────────
    Task<(IReadOnlyList<ProductDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? search, CancellationToken ct);

    /// <summary>Returns a projected ProductDto by ID, or null if not found.</summary>
    Task<ProductDto?> GetByIdDtoAsync(int id, CancellationToken ct);

    /// <summary>
    /// Loads the tracked entity, applies changes and saves.
    /// Sets OriginalValue of RowVersion so EF raises DbUpdateConcurrencyException on conflict.
    /// </summary>
    Task<UpdateResult> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct);

    /// <summary>Soft-deletes by setting IsDeleted = true.</summary>
    Task<bool> DeleteAsync(int id, CancellationToken ct);

    // ── CQRS: create ──────────────────────────────────────────────────────────
    /// <summary>Creates a new product and returns its generated ID.</summary>
    Task<int> CreateAsync(string name, decimal price, int categoryId, CancellationToken ct);
}
