using FluentValidation;

namespace VersioningDemo.Application.Products.Commands.CreateProduct;

/// <summary>
/// FluentValidation validator for CreateProductCommand.
/// Registered automatically by AddValidatorsFromAssembly and executed
/// by the ValidationPipelineBehaviour before the handler runs.
/// </summary>
public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("A valid CategoryId is required.");
    }
}
