using MediatR;
using VersioningDemo.Application.DTOs;

namespace VersioningDemo.Application.Products.Queries.GetAllProducts;

/// <summary>
/// CQRS Query: returns a paged list of products.
/// Queries are read-only — they never change state.
/// </summary>
public record GetAllProductsQuery(
    int PageNumber = 1,
    int PageSize   = 10,
    string? Search = null) : IRequest<PagedResult<ProductDto>>;
