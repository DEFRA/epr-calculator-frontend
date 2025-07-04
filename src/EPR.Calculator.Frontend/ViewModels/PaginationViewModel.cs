namespace EPR.Calculator.Frontend.ViewModels
{
    public record PaginationViewModel
    {
        public IEnumerable<object> Records { get; init; } = Enumerable.Empty<object>();

        public string? Caption { get; init; }

        public int CurrentPage { get; init; }

        public int PageSize { get; init; }

        public int TotalRecords { get; init; }

        public IEnumerable<int> PageSizeOptions { get; init; } = new[] { 10, 25, 50 };

        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

        public int StartRecord => TotalRecords == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;

        public int EndRecord => Math.Min(CurrentPage * PageSize, TotalRecords);

        // Route configuration
        public string RouteName { get; init; } = "index";

        public Dictionary<string, object> RouteValues { get; init; } = new();
    }
}
