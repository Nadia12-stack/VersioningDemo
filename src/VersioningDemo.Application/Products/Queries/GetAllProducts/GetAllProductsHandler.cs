using MediatR;
using VersioningDemo.Application.Common.Interfaces;
using VersioningDemo.Application.DTOs;

namespace VersioningDemo.Application.Products.Queries.GetAllProducts;

/// <summary>
/// Handles GetAllProductsQuery.
/// Delegates to IProductRepository to keep EF Core out of the Application layer.
/// </summary>
public sealed class GetAllProductsHandler(IProductRepository repository)
    : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(
        GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize   = Math.Clamp(request.PageSize, 1, 100);

        var (items, total) = await repository.GetPagedAsync(
            pageNumber, pageSize, request.Search, cancellationToken);

        return new PagedResult<ProductDto>(items, pageNumber, pageSize, total);
    }
}
