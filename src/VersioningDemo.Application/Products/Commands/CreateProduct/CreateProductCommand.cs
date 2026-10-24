using MediatR;

namespace VersioningDemo.Application.Products.Commands.CreateProduct;

/// <summary>
/// CQRS Command: create a new product.
/// Commands change state and return the ID of the created resource.
/// </summary>
public record CreateProductCommand(
    string Name,
    decimal Price,
    int CategoryId) : IRequest<int>;
