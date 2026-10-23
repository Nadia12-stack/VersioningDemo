using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using VersioningDemo.Application.Products;

namespace VersioningDemo.Api.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController(IProductService service) : ControllerBase
{
    // ── GET /api/v2/products?pageNumber=1&pageSize=10&search=lap ────────────
    /// <summary>
    /// Paginated product list.
    /// • AsNoTracking + Select → no change tracking overhead, single JOIN query.
    /// • search uses Name.StartsWith → exploits the Name index (seek, not scan).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> Get(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize   = 10,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
        => Ok(await service.GetAsync(pageNumber, pageSize, search, ct));

    // ── PUT /api/v2/products/{id} ────────────────────────────────────────────
    /// <summary>
    /// Updates a product using Change Tracking (no db.Update call).
    /// Sends 409 Conflict when the RowVersion the client sent is stale.
    /// Policy: client must re-fetch and re-submit.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, dto, ct);

        return result switch
        {
            UpdateResult.Success  => NoContent(),
            UpdateResult.NotFound => NotFound(),
            UpdateResult.Conflict => Problem(
                title: "Concurrency conflict",
                detail: "The product was modified by another user. Reload the data and try again.",
                statusCode: StatusCodes.Status409Conflict),
            _ => StatusCode(500)
        };
    }

    // ── DELETE /api/v2/products/{id} ─────────────────────────────────────────
    /// <summary>
    /// Soft-deletes the product (sets IsDeleted = true).
    /// The row is never physically removed; it is invisible to all normal queries
    /// due to the global HasQueryFilter. Use IgnoreQueryFilters() to see it.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await service.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}
