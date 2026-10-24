using MediatR;
using VersioningDemo.Application.Common.Interfaces;

namespace VersioningDemo.Application.Products.Commands.CreateProduct;

/// <summary>
/// Handles CreateProductCommand.
/// Creates the product via the repository and returns its new ID.
/// Validation runs BEFORE this handler (ValidationPipelineBehaviour in the MediatR pipeline).
/// </summary>
public sealed class CreateProductHandler(IProductRepository repository)
    : IRequestHandler<CreateProductCommand, int>
{
    public Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        => repository.CreateAsync(request.Name, request.Price, request.CategoryId, cancellationToken);
}
