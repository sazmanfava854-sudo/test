using HRPerformance.Interfaces;
using HRPerformance.Data;

namespace HRPerformance.Repositories;
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    public UnitOfWork(ApplicationDbContext context) => _context = context;
    public IRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T);
        if (!_repositories.ContainsKey(type)) _repositories[type] = new Repository<T>(_context);
        return (IRepository<T>)_repositories[type];
    }
    public async Task<int> SaveChangesAsync(CancellationToken ct = default) => await _context.SaveChangesAsync(ct);
    public void Dispose() => _context.Dispose();
}
