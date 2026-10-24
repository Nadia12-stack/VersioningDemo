using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VersioningDemo.Application.Common.Interfaces;
using VersioningDemo.Infrastructure.Persistence;
using VersioningDemo.Infrastructure.Persistence.Repositories;

namespace VersioningDemo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Development / testing: no connection string → InMemory.
            services.AddDbContext<AppDbContext>(o =>
                o.UseInMemoryDatabase("VersioningDemo"));
        }
        else
        {
            // Production: SQL Server.
            // dotnet ef migrations add Init -p Infrastructure -s Api
            // dotnet ef database update  -p Infrastructure -s Api
            services.AddDbContext<AppDbContext>(o =>
                o.UseSqlServer(connectionString));
        }

        // OCP / DIP: open-generic registration — any IRepository<T> resolves to Repository<T>
        // without touching this file when new entities are added.
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Product-specific repository (inherits from Repository<Product>)
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
