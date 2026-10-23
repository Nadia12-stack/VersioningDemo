using System.ComponentModel.DataAnnotations;

namespace VersioningDemo.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public bool IsDeleted { get; set; }           // Soft Delete

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];  // Concurrency Token
}
