using VersioningDemo.Domain.Entities;

namespace VersioningDemo.Api.Contracts.V1;

public static class ProductMappings
{
    public static ProductResponse ToV1(this Product p) => new(p.Id, p.Name, p.Price);
}
