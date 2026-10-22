# Versioning Demo: Products API (ASP.NET Core 8)

A small but production-minded example of **API Versioning** using `Asp.Versioning`,
organised with **Clean Architecture**.

## Getting started

```bash
dotnet new sln -n VersioningDemo
dotnet sln add src/*/*.csproj tests/*/*.csproj
dotnet run --project src/VersioningDemo.Api
```

Swagger UI: `/swagger` (one document per version: `v1`, `v2`).

| Version | Endpoint | Response |
|---------|----------|----------|
| v1 (deprecated) | `GET /api/v1/products` | `{ id, name, price }` |
| v2 | `GET /api/v2/products` | `{ id, name, price, categoryName }` |

Both also support `GET /{id}`.

## Architecture

```
Domain          Entities only (Product, Category)
Application     IProductService + repository abstraction (shared by ALL versions)
Infrastructure  EF Core + seed data
Api             Controllers + Contracts (DTOs + mappers) per version, Swagger
```

Key rule: **versions differ only in the Contract**. Business logic and data access are
written once and shared, so a pricing fix never has to be copied across versions.

## Versioning strategy

- **URL path versioning** (`/api/v{version}/...`): explicit, easy to test, easy to trace in logs.
- `AssumeDefaultVersionWhenUnspecified = false`: the API never silently serves a different version.
- Only **breaking contract changes** create a new version (removing/renaming a property,
  changing a type or its meaning). Adding an optional property or a new endpoint does not.
- Requesting an unsupported version (e.g. `/api/v3/...`) fails (404 with URL-segment versioning), never a fallback to v1.

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


