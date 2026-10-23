# Versioning Demo: Products API (ASP.NET Core 9)

A small but production-minded example of **API Versioning** using `Asp.Versioning`,
organised with **Clean Architecture**.

## Getting started

```bash
dotnet run --project src/VersioningDemo.Api
```

Swagger UI: `/swagger` (one document per version: `v1`, `v2`).

| Version | Endpoint | Response |
|---------|----------|----------|
| v1 (deprecated) | `GET /api/v1/products` | `{ id, name, price }` |
| v2 | `GET /api/v2/products` | paged `{ items[], pageNumber, pageSize, totalCount, totalPages }` |
| v2 | `PUT /api/v2/products/{id}` | 204 / 404 / **409 Conflict** |
| v2 | `DELETE /api/v2/products/{id}` | 204 / 404 (soft delete) |

## Architecture

```
Domain          Entities only (Product, Category)
Application     IProductService + IProductRepository abstractions (all versions share them)
Infrastructure  EF Core, ProductRepository, ProductConfiguration, AppDbContext
Api             Controllers + Contracts (DTOs + mappers) per version, Swagger
```

Key rule: **versions differ only in the Contract**. Business logic and data access are
written once and shared, so a pricing fix never has to be copied across versions.

---

## EF Core design decisions (v2)

### Why `AsNoTracking` + `Select` in GET?

`AsNoTracking()` tells EF to skip the identity map and snapshot machinery entirely —
no object-graph is cached, so memory and CPU footprint drop significantly for
read-only responses. Combined with `Select(p => new ProductDto(...))`, EF generates a
**single SQL query** that fetches only the columns the DTO needs. No lazy-loading, no
`Include` that might be ignored later.

### Why `Select` instead of `Include` here?

`Include(p => p.Category)` loads the full `Category` entity graph into memory for every
row. `Select(p => p.Category.Name)` projects to a scalar in the same query (EF emits
one `JOIN`). The result:

- Fewer columns over the wire.
- No N+1 problem: EF does **not** issue one query per product.
- Works correctly with pagination (`Skip` / `Take` run **before** the projection).

> **Tip:** Inspect the generated SQL during development with `query.ToQueryString()`.

### Why an index on `Name`?

`Name` is the column used in `WHERE Name LIKE 'search%'` (StartsWith). Without an index
EF would do a **full table scan** on every search request. A B-tree index converts that
to an **index seek** — O(log n) instead of O(n). Trade-off: extra storage per row and
a marginally slower `INSERT`/`UPDATE`; worthwhile only for columns that appear
frequently in `WHERE` or `ORDER BY` clauses.

```csharp
builder.HasIndex(p => p.Name);
```

### Why no `db.Update(product)` in PUT?

`db.Update(entity)` marks **every property** of the entity as modified, generating an
`UPDATE` that writes all columns — even ones the client didn't touch. Instead we:

1. Load the entity with `FirstOrDefaultAsync` → EF starts tracking it.
2. Set only `Name` and `Price` → Change Tracking detects exactly those two mutations.
3. Call `SaveChangesAsync` → EF emits a minimal `UPDATE` for only the changed columns.

This is safer (no accidental overwrite of unrelated fields) and produces tighter SQL.

### Concurrency conflict policy: 409 + client reload

Each `Product` row carries a `RowVersion` (`byte[]`, `[Timestamp]`). When the client
sends a `PUT` it includes the `RowVersion` it read earlier. We set that value as
`OriginalValue` so EF appends `WHERE RowVersion = @original` to the `UPDATE`.

If another user already committed a change the `WHERE` matches zero rows →
`DbUpdateConcurrencyException` → we return **409 Conflict** with an explanatory message.

**Policy:** The client must reload the current data and retry. We do **not** silently
merge or overwrite, because we don't know which change should win.

```csharp
db.Entry(product).Property(p => p.RowVersion).OriginalValue = dto.RowVersion;
```

### Soft Delete

`IsDeleted = true` replaces a physical `DELETE`. The EF global query filter

```csharp
builder.HasQueryFilter(p => !p.IsDeleted);
```

automatically appends `WHERE IsDeleted = 0` to every query, so deleted products are
invisible to all application code without any extra effort. To see them (admin / audit
screens) call `.IgnoreQueryFilters()` explicitly.

---

## Versioning strategy

- **URL path versioning** (`/api/v{version}/...`): explicit, easy to test, easy to trace in logs.
- `AssumeDefaultVersionWhenUnspecified = false`: the API never silently serves a different version.
- Only **breaking contract changes** create a new version (removing/renaming a property,
  changing a type or its meaning). Adding an optional property or a new endpoint does not.
- Requesting an unsupported version (e.g. `/api/v3/...`) fails (404), never a fallback to v1.

## Deprecation policy

1. Announce the deprecation (`[ApiVersion("1.0", Deprecated = true)]`). Responses carry
   `api-supported-versions` and `api-deprecated-versions` headers (`ReportApiVersions = true`),
   and Swagger marks the version as deprecated.
2. Monitor usage of the old version in production.
3. Keep it running for the agreed support window.
4. Retire it only after clients have migrated and a removal date was communicated.

## Testing

```bash
dotnet test
```

Integration tests (`WebApplicationFactory`) lock the contract of each version:
v1 has no `categoryName`, v2 does, unknown products return 404 in both,
unsupported versions fail, and v1 is reported as deprecated.

## Docker

```bash
docker build -t versioning-demo .
docker run -p 8080:8080 versioning-demo
```

## CI

GitHub Actions (`.github/workflows/ci.yml`) restores, builds and runs the tests on every push and PR.

Made by Nadia Emad Fawzy
