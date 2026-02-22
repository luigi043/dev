using System.Linq.Expressions;

namespace InsureX.Domain.Interfaces;

/// <summary>
/// Generic repository interface for data access operations
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    #region Get Methods
    
    /// <summary>
    /// Gets an entity by integer ID
    /// </summary>
    /// <param name="id">The integer identifier</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Gets an entity by GUID ID
    /// </summary>
    /// <param name="id">The GUID identifier</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <returns>Collection of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Finds entities matching the predicate
    /// </summary>
    /// <param name="predicate">The filter condition</param>
    /// <returns>Collection of matching entities</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Gets the first entity matching the predicate
    /// </summary>
    /// <param name="predicate">The filter condition</param>
    /// <returns>The first matching entity or null</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    #endregion

    #region Pagination Methods

    /// <summary>
    /// Gets paged results with optional filtering and sorting
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="predicate">Optional filter condition</param>
    /// <param name="orderBy">Optional sorting function</param>
    /// <returns>Paged collection of entities</returns>
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, 
        Expression<Func<T, bool>>? predicate = null, 
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);

    #endregion

    #region Count & Existence Methods

    /// <summary>
    /// Checks if any entity matches the predicate
    /// </summary>
    /// <param name="predicate">The filter condition</param>
    /// <returns>True if matching entities exist</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Counts total entities or those matching a predicate
    /// </summary>
    /// <param name="predicate">Optional filter condition</param>
    /// <returns>The count of entities</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    #endregion

    #region Write Operations

    /// <summary>
    /// Adds a new entity (without saving)
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity with generated IDs</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Adds multiple entities (without saving)
    /// </summary>
    /// <param name="entities">The entities to add</param>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Updates an existing entity (without saving)
    /// </summary>
    /// <param name="entity">The entity to update</param>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Updates multiple entities (without saving)
    /// </summary>
    /// <param name="entities">The entities to update</param>
    Task UpdateRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Deletes an entity (without saving)
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Deletes multiple entities (without saving)
    /// </summary>
    /// <param name="entities">The entities to delete</param>
    Task DeleteRangeAsync(IEnumerable<T> entities);

    #endregion

    #region Persistence

    /// <summary>
    /// Saves all changes made in this repository
    /// </summary>
    /// <returns>The number of affected rows</returns>
    Task<int> SaveChangesAsync();

    #endregion

    #region Queryable

    /// <summary>
    /// Gets an IQueryable for advanced queries
    /// </summary>
    /// <returns>IQueryable of the entity</returns>
    IQueryable<T> GetQueryable();

    #endregion

    #region Utility Methods

    /// <summary>
    /// Detaches an entity from the context
    /// </summary>
    /// <param name="entity">The entity to detach</param>
    void Detach(T entity);

    /// <summary>
    /// Reloads an entity from the database
    /// </summary>
    /// <param name="entity">The entity to reload</param>
    Task ReloadAsync(T entity);

    #endregion
}