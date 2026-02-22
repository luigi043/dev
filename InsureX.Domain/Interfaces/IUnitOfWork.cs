namespace InsureX.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Repository access
    IRepository<T> Repository<T>() where T : class;
    
    // Transaction management
    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    
    // Batch operations
    Task ExecuteInTransactionAsync(Func<Task> operation);
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation);
    
    // State management
    bool HasChanges();
    void Clear();
}