using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using VersioningDemo.Application.Common.Behaviours;
using VersioningDemo.Application.Services;

namespace VersioningDemo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ── Legacy service (V1 pass-through) ────────────────────────────────
        services.AddScoped<IProductService, ProductService>();

        // ── MediatR: scans this assembly for IRequestHandler<,> ─────────────
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // ── FluentValidation: scans this assembly for AbstractValidator<T> ───
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // ── Validation pipeline: runs before every handler ───────────────────
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehaviour<,>));

        return services;
    }
}
