using MediatR;
using VersioningDemo.Application.Common.Interfaces;

namespace VersioningDemo.Application.Products.Commands.DeleteProduct;

public record DeleteProductCommand(int Id) : IRequest<bool>;

public sealed class DeleteProductHandler(IProductRepository repository)
    : IRequestHandler<DeleteProductCommand, bool>
{
    public Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        => repository.DeleteAsync(request.Id, cancellationToken);
}
