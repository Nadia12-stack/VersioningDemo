using System.Linq.Expressions;

namespace VersioningDemo.Application.Common.Interfaces;

/// <summary>
/// Generic repository abstraction — SRP: single persistence contract shared by all entities.
/// Keeps Application layer independent of EF Core (ISP: only the methods callers actually need).
/// </summary>
public interface IRepository<T> where T : class
{
    Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default);

    Task<T> AddAsync(
        T entity,
        CancellationToken cancellationToken = default);

    void Update(T entity);

    void Delete(T entity);

    Task<IEnumerable<T>> GetAsync(
        Expression<Func<T, bool>>? expression = null,
        Expression<Func<T, object>>[]? includes = null,
        bool tracked = true,
        CancellationToken cancellationToken = default);

    Task<T?> GetOneAsync(
        Expression<Func<T, bool>>? expression = null,
        Expression<Func<T, object>>[]? includes = null,
        bool tracked = true,
        CancellationToken cancellationToken = default);

    Task CommitAsync(CancellationToken cancellationToken = default);
}
