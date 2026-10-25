using Microsoft.Extensions.Logging;
using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Application.Products;

public sealed class ProductService(IProductRepository repository, ILogger<ProductService> logger) : IProductService
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        logger.LogDebug("Fetching all products");

        var products = await repository.GetAllAsync(cancellationToken);

        logger.LogInformation("Fetched {Count} products", products.Count);
        return products;
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        logger.LogDebug("Fetching product {ProductId}", id);

        var product = await repository.GetByIdAsync(id, cancellationToken);

        if (product is null)
            logger.LogWarning("Product {ProductId} not found", id);

        return product;
    }
}
