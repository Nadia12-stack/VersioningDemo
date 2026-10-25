using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using VersioningDemo.Application.DTOs;
using VersioningDemo.Application.Products.Commands.CreateProduct;
using VersioningDemo.Application.Products.Queries.GetAllProducts;
using VersioningDemo.Application.Products.Queries.GetProductById;

namespace VersioningDemo.Api.Controllers.V2;

/// <summary>
/// V2 Products controller — uses CQRS (MediatR) instead of IProductService.
///
/// Each action dispatches a Query or Command through ISender.
/// The MediatR pipeline runs FluentValidation before every handler.
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController(ISender sender) : ControllerBase
{
    // ── GET /api/v2/products?pageNumber=1&pageSize=10&search=lap ─────────────
    /// <summary>
    /// Paginated, searchable product list.
    /// Query → GetAllProductsHandler → IProductRepository.GetPagedAsync
    /// (AsNoTracking + Select, no N+1, Name index used for search).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize   = 10,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetAllProductsQuery(pageNumber, pageSize, search), ct);
        return Ok(result);
    }

    // ── GET /api/v2/products/{id} ────────────────────────────────────────────
    /// <summary>
    /// Fetch a single product by ID.
    /// Returns 404 when not found or soft-deleted.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken ct)
    {
        var product = await sender.Send(new GetProductByIdQuery(id), ct);
        return product is null ? NotFound() : Ok(product);
    }

    // ── POST /api/v2/products ────────────────────────────────────────────────
    /// <summary>
    /// Create a new product.
    /// FluentValidation runs automatically via the MediatR pipeline before the handler.
    /// Returns 201 Created with the Location header pointing to the new resource.
    /// Returns 400 Bad Request with validation error details on invalid input.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductCommand command, CancellationToken ct)
    {
        try
        {
            var id = await sender.Send(command, ct);
            return CreatedAtAction(nameof(GetById), new { id, version = "2" }, new { id });
        }
        catch (ValidationException ex)
        {
            // Convert FluentValidation errors → standard ASP.NET Core ValidationProblemDetails
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            return ValidationProblem(ModelState);
        }
    }

    // ── PUT /api/v2/products/{id} ────────────────────────────────────────────
    /// <summary>
    /// Updates a product using Change Tracking (no db.Update call).
    /// Returns 409 Conflict when RowVersion is stale.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id, [FromBody] UpdateProductDto dto, CancellationToken ct)
    {
        var result = await sender.Send(
            new Application.Products.Commands.UpdateProduct.UpdateProductCommand(id, dto), ct);

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
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await sender.Send(
            new Application.Products.Commands.DeleteProduct.DeleteProductCommand(id), ct);
        return deleted ? NoContent() : NotFound();
    }
}
