using InsureX.Domain.Interfaces;

namespace InsureX.Infrastructure.Extensions;

public static class RepositoryExtensions
{
    /// <summary>
    /// Gets paged results with tenant isolation
    /// </summary>
    public static async Task<IEnumerable<T>> GetPagedByTenantAsync<T>(
        this IRepository<T> repository,
        Guid tenantId,
        int page,
        int pageSize,
        System.Linq.Expressions.Expression<Func<T, bool>>? additionalFilter = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null) where T : class, ITenantScoped
    {
        System.Linq.Expressions.Expression<Func<T, bool>> tenantFilter = 
            e => e.TenantId == tenantId;

        var finalFilter = additionalFilter != null
            ? CombineExpressions(tenantFilter, additionalFilter)
            : tenantFilter;

        return await repository.GetPagedAsync(page, pageSize, finalFilter, orderBy);
    }

    private static System.Linq.Expressions.Expression<Func<T, bool>> CombineExpressions<T>(
        System.Linq.Expressions.Expression<Func<T, bool>> expr1,
        System.Linq.Expressions.Expression<Func<T, bool>> expr2)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);

        var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);

        return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(
            System.Linq.Expressions.Expression.AndAlso(left, right), parameter);
    }

    private class ReplaceExpressionVisitor
        : System.Linq.Expressions.ExpressionVisitor
    {
        private readonly System.Linq.Expressions.Expression _oldValue;
        private readonly System.Linq.Expressions.Expression _newValue;

        public ReplaceExpressionVisitor(System.Linq.Expressions.Expression oldValue, System.Linq.Expressions.Expression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override System.Linq.Expressions.Expression Visit(System.Linq.Expressions.Expression? node)
        {
            return node == _oldValue ? _newValue : base.Visit(node)!;
        }
    }
}