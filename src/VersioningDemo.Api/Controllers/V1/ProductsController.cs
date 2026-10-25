using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VersioningDemo.Api.Contracts.V1;
using VersioningDemo.Application.Services;

namespace VersioningDemo.Api.Controllers.V1;

/// <summary>Deprecated: still served during the support window. Use v2.</summary>
[ApiController]
[ApiVersion("1.0", Deprecated = true)]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController(IProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(CancellationToken ct)
    {
        var products = await productService.GetAllAsync(ct);
        return Ok(products.Select(p => p.ToV1()).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken ct)
    {
        var product = await productService.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(product.ToV1());
    }
}
