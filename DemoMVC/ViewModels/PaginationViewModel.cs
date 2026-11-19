namespace DemoMVC.ViewModels;

public sealed class PaginationViewModel
{
    public required int CurrentPage { get; init; }
    public required int TotalPages { get; init; }
    public required int TotalItems { get; init; }
    public required int PageSize { get; init; }
    public required int FirstItem { get; init; }
    public required int LastItem { get; init; }
    public required string Action { get; init; }
    public string? SearchString { get; init; }
    public Dictionary<string, object?> RouteValues { get; init; } = new();
}
