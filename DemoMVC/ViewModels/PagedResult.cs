using Microsoft.EntityFrameworkCore;

namespace DemoMVC.ViewModels;

public sealed class PagedResult<T>
{
    public const int DefaultPageSize = 10;

    public required IReadOnlyList<T> Items { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public int FirstItem => TotalItems == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    public int LastItem => TotalItems == 0 ? 0 : Math.Min(CurrentPage * PageSize, TotalItems);

    public static async Task<PagedResult<T>> CreateAsync(
        IQueryable<T> query,
        int page,
        int pageSize = DefaultPageSize)
    {
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        if (totalPages == 0)
        {
            totalPages = 1;
        }

        var currentPage = Math.Clamp(page, 1, totalPages);
        var items = await query
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>
        {
            Items = items,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}
