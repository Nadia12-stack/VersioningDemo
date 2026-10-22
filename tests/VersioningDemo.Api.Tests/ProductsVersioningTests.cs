using System.Net;
using Xunit;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VersioningDemo.Api.Tests;

/// <summary>
/// Contract tests: each version must keep returning ITS OWN shape.
/// </summary>
public class ProductsVersioningTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<JsonDocument> GetJsonAsync(string url)
    {
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task V1_GetById_ReturnsOldContract_WithoutCategoryName()
    {
        using var doc = await GetJsonAsync("/api/v1/products/1");
        var root = doc.RootElement;

        Assert.Equal("Laptop", root.GetProperty("name").GetString());
        Assert.Equal(JsonValueKind.Number, root.GetProperty("price").ValueKind);
        Assert.False(root.TryGetProperty("categoryName", out _));
    }

    [Fact]
    public async Task V2_GetById_ReturnsNewContract_WithCategoryName()
    {
        using var doc = await GetJsonAsync("/api/v2/products/1");
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Number, root.GetProperty("price").ValueKind);
        Assert.Equal("Electronics", root.GetProperty("categoryName").GetString());
    }

    [Fact]
    public async Task V1_GetAll_ReturnsOldContract()
    {
        using var doc = await GetJsonAsync("/api/v1/products");
        var first = doc.RootElement[0];

        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.False(first.TryGetProperty("categoryName", out _));
    }

    [Fact]
    public async Task V2_GetAll_ReturnsNewContract()
    {
        using var doc = await GetJsonAsync("/api/v2/products");

        Assert.All(doc.RootElement.EnumerateArray(),
            item => Assert.True(item.TryGetProperty("categoryName", out _)));
    }

    [Theory]
    [InlineData("/api/v1/products/999")]
    [InlineData("/api/v2/products/999")]
    public async Task UnknownProduct_Returns404_InBothVersions(string url)
    {
        var response = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UnsupportedVersion_Fails_AndNeverFallsBackToAnotherVersion()
    {
        var response = await _client.GetAsync("/api/v3/products/1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task V1_IsReportedAsDeprecated_ViaResponseHeaders()
    {
        var response = await _client.GetAsync("/api/v1/products/1");

        Assert.True(response.Headers.Contains("api-deprecated-versions"));
        Assert.True(response.Headers.Contains("api-supported-versions"));
    }
}


