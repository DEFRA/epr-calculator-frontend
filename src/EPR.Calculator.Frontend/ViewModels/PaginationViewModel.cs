using EPR.Calculator.Frontend.Constants;

namespace EPR.Calculator.Frontend.ViewModels;

public record PaginationViewModel : BasePaginationModel
{
    public IEnumerable<object> Records { get; init; } = Enumerable.Empty<object>();
    public int Total { get; init; }
    public IEnumerable<int> PageSizeOptions { get; init; } = CommonConstants.PageSizeOptions;
    public int BlockSize { get; init; } = CommonConstants.DefaultBlockSize;

    // Route configuration
    public string RouteName { get; init; } = "index";

    public RouteValueDictionary RouteValues { get; init; } = new();

    // Calculated properties
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    public int StartRecord => Total == 0 ? 0 : (Page - 1) * PageSize + 1;
    public int EndRecord => Math.Min(Page * PageSize, Total);
}

public record BasePaginationModel
{
    private readonly int page = CommonConstants.DefaultPage;
    private readonly int pageSize = CommonConstants.DefaultPageSize;

    public int Page
    {
        get => page;
        init => page = value > 0 ? value : CommonConstants.DefaultPage;
    }

    public int PageSize
    {
        get => pageSize;
        init => pageSize = value > 0 ? value : CommonConstants.DefaultPageSize;
    }
}
