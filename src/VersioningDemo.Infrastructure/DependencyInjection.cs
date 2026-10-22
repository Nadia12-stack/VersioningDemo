using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VersioningDemo.Application.Products;
using VersioningDemo.Infrastructure.Persistence;

namespace VersioningDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // InMemory keeps the demo zero-setup. Swap for UseSqlServer / UseSqlite in real projects.
        services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("VersioningDemo"));
        services.AddScoped<IProductRepository, ProductRepository>();
        return services;
    }
}
