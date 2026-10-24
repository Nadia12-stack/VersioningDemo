using MediatR;
using VersioningDemo.Application.Common.Interfaces;
using VersioningDemo.Application.DTOs;

namespace VersioningDemo.Application.Products.Queries.GetProductById;

/// <summary>
/// Handles GetProductByIdQuery.
/// Returns null when the product is not found; the controller maps that to 404.
/// </summary>
public sealed class GetProductByIdHandler(IProductRepository repository)
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public Task<ProductDto?> Handle(
        GetProductByIdQuery request, CancellationToken cancellationToken)
        => repository.GetByIdDtoAsync(request.Id, cancellationToken);
}
