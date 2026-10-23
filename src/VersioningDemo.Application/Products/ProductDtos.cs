namespace VersioningDemo.Application.Products;

/// <summary>Flat read DTO returned by the paginated GET endpoint.</summary>
public record ProductDto(int Id, string Name, decimal Price, string CategoryName, byte[] RowVersion);

/// <summary>
/// Payload sent by the client on PUT.
/// RowVersion must match the value the client read earlier — used for optimistic concurrency.
/// </summary>
public record UpdateProductDto(string Name, decimal Price, byte[] RowVersion);

/// <summary>Generic paged result wrapper.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount)
{
    /// <summary>Total number of pages given the current page size.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>Outcome of an update operation.</summary>
public enum UpdateResult
{
    Success,
    NotFound,
    Conflict
}
