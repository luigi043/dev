using Microsoft.EntityFrameworkCore;
using InsureX.Domain.Interfaces;
using InsureX.Infrastructure.Data;
using System.Linq.Expressions;

namespace InsureX.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(AppDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Id must be greater than zero", nameof(id));
            
        return await _dbSet.FindAsync(id);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));
            
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
            
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
            
        return await _dbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<T> AddAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
            
        var entry = await _dbSet.AddAsync(entity);
        return entry.Entity;
    }

    public Task UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
            
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
            
        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));
            
        return await _dbSet.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _dbSet.CountAsync();
            
        return await _dbSet.CountAsync(predicate);
    }

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Concurrency conflict detected", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Database update error occurred", ex);
        }
    }

    public async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, 
        Expression<Func<T, bool>>? predicate = null, 
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        
        var query = _dbSet.AsQueryable();
        
        if (predicate != null)
            query = query.Where(predicate);
        
        if (orderBy != null)
            query = orderBy(query);
        
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public IQueryable<T> GetQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));
            
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await action();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public void Detach(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
            
        _context.Entry(entity).State = EntityState.Detached;
    }

    public async Task ReloadAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));
            
        await _context.Entry(entity).ReloadAsync();
    }
}