using EPR.Calculator.Frontend.Constants;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record PaginationViewModel
    {
        public IEnumerable<object> Records { get; init; } = Enumerable.Empty<object>();

        public string? Caption { get; init; }

        public int CurrentPage { get; init; }

        public int PageSize { get; init; }

        public int TotalRecords { get; init; }

        public IEnumerable<int> PageSizeOptions { get; init; } = CommonConstants.PageSizeOptions;

        public int BlockSize { get; init; } = CommonConstants.DefaultBlockSize;

        public int TotalPages => (int)Math.Ceiling((double)this.TotalRecords / this.PageSize);

        public int StartRecord => this.TotalRecords == 0 ? 0 : ((this.CurrentPage - 1) * this.PageSize) + 1;

        public int EndRecord => Math.Min(this.CurrentPage * this.PageSize, this.TotalRecords);

        // Route configuration
        public string RouteName { get; init; } = "index";

        public Dictionary<string, object?> RouteValues { get; init; } = new();
    }
}
