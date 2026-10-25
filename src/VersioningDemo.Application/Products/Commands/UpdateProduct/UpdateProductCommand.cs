using MediatR;
using VersioningDemo.Application.Common.Interfaces;
using VersioningDemo.Application.DTOs;

namespace VersioningDemo.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand(int Id, UpdateProductDto Dto) : IRequest<UpdateResult>;

public sealed class UpdateProductHandler(IProductRepository repository)
    : IRequestHandler<UpdateProductCommand, UpdateResult>
{
    public Task<UpdateResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        => repository.UpdateAsync(request.Id, request.Dto, cancellationToken);
}
