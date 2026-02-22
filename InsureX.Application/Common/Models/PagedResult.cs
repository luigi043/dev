namespace InsureX.Application.Common.Models;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
    public int FirstItem => ((Page - 1) * PageSize) + 1;
    public int LastItem => Math.Min(Page * PageSize, TotalItems);
    public bool IsEmpty => !Items.Any();
    
    // Optional: Add a method to create a PagedResult from an IQueryable
    public static async Task<PagedResult<T>> CreateAsync(
        IQueryable<T> source, 
        int page, 
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    // Optional: Add a method to create an empty PagedResult
    public static PagedResult<T> Empty(int page = 1, int pageSize = 25)
    {
        return new PagedResult<T>
        {
            Items = new List<T>(),
            Page = page,
            PageSize = pageSize,
            TotalItems = 0,
            TotalPages = 0
        };
    }

    // Optional: Add a method to map items to a different type
    public PagedResult<TDest> Map<TDest>(Func<T, TDest> mapper)
    {
        return new PagedResult<TDest>
        {
            Items = Items.Select(mapper),
            Page = Page,
            PageSize = PageSize,
            TotalItems = TotalItems,
            TotalPages = TotalPages
        };
    }
}

// Add these using directives at the top of your file if using the CreateAsync method
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;