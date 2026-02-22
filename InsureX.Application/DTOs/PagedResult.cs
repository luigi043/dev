using System;
using System.Collections.Generic;

namespace InsureX.Application.DTOs
{
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
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages =>
            PageSize == 0
                ? 0
                : (int)Math.Ceiling(TotalItems / (double)PageSize);

        /// <summary>
        /// Indicates if the result contains any items.
        /// </summary>
        public bool HasItems => Items.Count > 0;

        public PagedResult() { }

        public PagedResult(List<T> items, int totalItems, int page, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalItems = totalItems;
            Page = page;
            PageSize = pageSize;
        }
    }
}namespace InsureX.Application.DTOs;

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious => Page > 1;
}