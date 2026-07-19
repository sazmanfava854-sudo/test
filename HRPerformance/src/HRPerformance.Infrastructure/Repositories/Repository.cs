using HRPerformance.Domain.Interfaces;
using HRPerformance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRPerformance.Infrastructure.Repositories;
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;
    public Repository(ApplicationDbContext context) { _context = context; _dbSet = context.Set<T>(); }
    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) => await _dbSet.FindAsync(new object[] { id }, ct);
    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) => await _dbSet.ToListAsync(ct);
    public virtual async Task<IReadOnlyList<T>> FindAsync(System.Linq.Expressions.Expression<Func<T, bool>> predicate, CancellationToken ct = default) => await _dbSet.Where(predicate).ToListAsync(ct);
    public virtual async Task<T> AddAsync(T entity, CancellationToken ct = default) { await _dbSet.AddAsync(entity, ct); return entity; }
    public virtual Task UpdateAsync(T entity, CancellationToken ct = default) { _dbSet.Update(entity); return Task.CompletedTask; }
    public virtual Task DeleteAsync(T entity, CancellationToken ct = default) { _dbSet.Remove(entity); return Task.CompletedTask; }
    public IQueryable<T> Query() => _dbSet.AsQueryable();
}
