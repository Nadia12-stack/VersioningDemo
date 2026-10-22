using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Api.Contracts.V2;

public static class ProductMappings
{
    public static ProductResponse ToV2(this Product p) => new(p.Id, p.Name, p.Price, p.Category.Name);
}
