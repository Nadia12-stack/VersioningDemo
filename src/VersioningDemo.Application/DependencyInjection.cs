using Microsoft.Extensions.DependencyInjection;
using VersioningDemo.Application.Products;

namespace VersioningDemo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        return services;
    }
}
