using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Application.Products;

/// <summary>
/// Shared by ALL API versions. Versions differ only in the Contract (DTOs),
/// never in the business logic.
/// </summary>
public interface IProductService
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken);
}
