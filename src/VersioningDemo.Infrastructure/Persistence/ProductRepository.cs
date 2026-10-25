using Microsoft.EntityFrameworkCore;
using VersioningDemo.Application.Common.Interfaces;
using VersioningDemo.Application.DTOs;
using VersioningDemo.Domain.Entities;
using VersioningDemo.Infrastructure.Persistence.Repositories;

namespace VersioningDemo.Infrastructure.Persistence;

/// <summary>
/// Concrete product repository.
/// Inherits all generic CRUD from Repository&lt;Product&gt; (OCP / LSP).
/// Only overrides or adds product-specific behaviour (SRP: one reason to change per method).
/// DIP: consumers depend on IProductRepository, not this class.
/// </summary>
public sealed class ProductRepository : Repository<Product>, IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db) : base(db)
    {
        _db = db;
    }

    // ── V1 ───────────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
        => await _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => _db.Products.AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    // ── V2: paged read ────────────────────────────────────────────────────────

    /// <summary>
    /// AsNoTracking + Select → single JOIN query, no N+1.
    /// Name.StartsWith(search) exploits the Name index (index seek, not scan).
    /// </summary>
    public async Task<(IReadOnlyList<ProductDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? search, CancellationToken ct)
    {
        var query = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.StartsWith(search));

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Category.Name, p.RowVersion))
            .ToListAsync(ct);

        return (items, total);
    }

    /// <summary>Single-row projection by ID. Returns null when not found or soft-deleted.</summary>
    public Task<ProductDto?> GetByIdDtoAsync(int id, CancellationToken ct)
        => _db.Products.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto(p.Id, p.Name, p.Price, p.Category.Name, p.RowVersion))
            .FirstOrDefaultAsync(ct);

    // ── V2: update with concurrency ───────────────────────────────────────────

    /// <summary>
    /// Loads the tracked entity then mutates only the allowed fields.
    /// OriginalValue of RowVersion forces EF to append WHERE RowVersion = @original
    /// so a stale write raises DbUpdateConcurrencyException → 409 Conflict.
    /// We deliberately do NOT call _dbSet.Update() — Change Tracking detects mutations
    /// and emits a minimal UPDATE for only the changed columns.
    /// </summary>
    public async Task<UpdateResult> UpdateAsync(int id, UpdateProductDto dto, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null) return UpdateResult.NotFound;

        product.Name  = dto.Name;
        product.Price = dto.Price;

        _db.Entry(product).Property(p => p.RowVersion).OriginalValue = dto.RowVersion;

        try
        {
            await CommitAsync(ct);
            return UpdateResult.Success;
        }
        catch (DbUpdateConcurrencyException)
        {
            return UpdateResult.Conflict;
        }
    }

    // ── V2: soft delete ────────────────────────────────────────────────────────

    /// <summary>
    /// Sets IsDeleted = true — never physically removes the row.
    /// The global HasQueryFilter hides soft-deleted rows from all normal queries.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (product is null) return false;

        product.IsDeleted = true;
        await CommitAsync(ct);
        return true;
    }

    // ── CQRS: create ──────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a new product via the generic AddAsync, then persists via CommitAsync.
    /// Both methods are inherited from Repository&lt;Product&gt;.
    /// </summary>
    public async Task<int> CreateAsync(
        string name, decimal price, int categoryId, CancellationToken ct)
    {
        var product = new Product
        {
            Name       = name,
            Price      = price,
            CategoryId = categoryId
        };

        await AddAsync(product, ct);
        await CommitAsync(ct);
        return product.Id;
    }
}
