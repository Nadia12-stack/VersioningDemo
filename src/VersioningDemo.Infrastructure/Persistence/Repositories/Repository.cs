using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VersioningDemo.Application.Common.Interfaces;
using VersioningDemo.Infrastructure.Persistence;

namespace VersioningDemo.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic EF Core repository — OCP: new entities are supported without touching existing code.
/// DIP: application code depends on IRepository&lt;T&gt;, never on this class directly.
/// </summary>
public class Repository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet   = _context.Set<T>();
    }

    public async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
    {
        await _context.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<T> AddAsync(
        T entity,
        CancellationToken cancellationToken = default)
    {
        var result = await _dbSet.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<IEnumerable<T>> GetAsync(
        Expression<Func<T, bool>>? expression = null,
        Expression<Func<T, object>>[]? includes = null,
        bool tracked = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> entities = _dbSet.AsQueryable();

        if (expression is not null)
            entities = entities.Where(expression);

        if (includes is not null)
        {
            foreach (var include in includes)
                entities = entities.Include(include);
        }

        if (!tracked)
            entities = entities.AsNoTracking();

        return await entities.ToListAsync(cancellationToken);
    }

    public async Task<T?> GetOneAsync(
        Expression<Func<T, bool>>? expression = null,
        Expression<Func<T, object>>[]? includes = null,
        bool tracked = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<T> entities = _dbSet.AsQueryable();

        if (expression is not null)
            entities = entities.Where(expression);

        if (includes is not null)
        {
            foreach (var include in includes)
                entities = entities.Include(include);
        }

        if (!tracked)
            entities = entities.AsNoTracking();

        return await entities.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
