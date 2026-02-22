namespace InsureX.Application.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalItems { get; set; }
    
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
    public bool HasItems => Items.Any();

    public PagedResult() { }

    public PagedResult(List<T> items, int totalItems, int page, int pageSize)
    {
        Items = items ?? new List<T>();
        TotalItems = totalItems;
        Page = page;
        PageSize = pageSize;
    }

    public static PagedResult<T> Empty() => new()
    {
        Items = new List<T>(),
        TotalItems = 0,
        Page = 1,
        PageSize = 25
    };
}