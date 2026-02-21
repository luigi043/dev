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
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Total number of items across all pages.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-based).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        public int TotalPages 
        { 
            get
            {
                if (PageSize == 0) return 0;
                return (int)Math.Ceiling(TotalCount / (double)PageSize);
            }
        }

        /// <summary>
        /// Convenience method to check if there are any items.
        /// </summary>
        public bool HasItems => Items != null && Items.Count > 0;

        public PagedResult() { }

        public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }
}