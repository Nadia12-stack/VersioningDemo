using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Application.Products;

public sealed class ProductService(IProductRepository repository) : IProductService
{
    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
        => repository.GetAllAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => repository.GetByIdAsync(id, cancellationToken);
}
