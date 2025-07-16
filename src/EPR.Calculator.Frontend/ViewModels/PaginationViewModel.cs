using EPR.Calculator.Frontend.Constants;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record PaginationViewModel
    {
        public IEnumerable<object> Records { get; init; } = Enumerable.Empty<object>();

        public string? Caption { get; init; }

        public int CurrentPage { get; init; }

        public int PageSize { get; init; }

        public int TotalTableRecords { get; init; }

        public IEnumerable<int> PageSizeOptions { get; init; } = CommonConstants.PageSizeOptions;

        public int BlockSize { get; init; } = CommonConstants.DefaultBlockSize;

        // Route configuration
        public string RouteName { get; init; } = "index";

        public Dictionary<string, object?> RouteValues { get; init; } = new();

        // Calculated properties
        public int TotalPages => (int)Math.Ceiling((double)this.TotalTableRecords / this.PageSize);

        public int StartRecord => this.TotalTableRecords == 0 ? 0 : ((this.CurrentPage - 1) * this.PageSize) + 1;

        public int EndRecord => Math.Min(this.CurrentPage * this.PageSize, this.TotalTableRecords);
    }
}
