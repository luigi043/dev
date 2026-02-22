using System;
using System.Collections.Generic;
using System.Linq;

namespace InsureX.Application.DTOs;

/// <summary>
/// Represents a paged result set for queries.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The items in the current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 25;

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => 
        PageSize == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    /// <summary>
    /// Indicates if there is a next page.
    /// </summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>
    /// Indicates if there is a previous page.
    /// </summary>
    public bool HasPrevious => Page > 1;

    /// <summary>
    /// Indicates if the result contains any items.
    /// </summary>
    public bool HasItems => Items.Any();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PagedResult() { }

    /// <summary>
    /// Constructor with parameters.
    /// </summary>
    public PagedResult(List<T> items, int totalItems, int page, int pageSize)
    {
        Items = items ?? new List<T>();
        TotalItems = totalItems;
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Creates an empty paged result.
    /// </summary>
    public static PagedResult<T> Empty() => new()
    {
        Items = new List<T>(),
        TotalItems = 0,
        Page = 1,
        PageSize = 25
    };
}