using MediatR;
using VersioningDemo.Application.DTOs;

namespace VersioningDemo.Application.Products.Queries.GetProductById;

/// <summary>
/// CQRS Query: fetch a single product by its ID.
/// Returns null when not found — the controller converts that to 404.
/// </summary>
public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;
