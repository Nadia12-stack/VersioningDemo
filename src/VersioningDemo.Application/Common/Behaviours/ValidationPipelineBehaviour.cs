using FluentValidation;
using MediatR;

namespace VersioningDemo.Application.Common.Behaviours;

/// <summary>
/// MediatR Pipeline Behaviour that runs FluentValidation validators
/// before the request handler executes.
///
/// Flow: Controller → ISender.Send() → [ValidationPipelineBehaviour] → Handler
///
/// If validation fails, throws ValidationException which the global exception
/// handler (or a middleware) converts to 400 Bad Request with the error details.
/// </summary>
public sealed class ValidationPipelineBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
