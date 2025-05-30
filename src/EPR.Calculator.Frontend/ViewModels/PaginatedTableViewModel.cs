namespace EPR.Calculator.Frontend.ViewModels
{
    public record PaginatedTableViewModel
    {
        public IEnumerable<object> Records { get; init; }

        public string? Caption { get; init; }

        public int CurrentPage { get; init; }

        public int PageSize { get; init; }

        public int TotalRecords { get; init; }

        public IEnumerable<int> PageSizeOptions { get; init; } = new[] { 10, 25, 50 };

        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

        public int StartRecord { get; init; }

        public int EndRecord { get; init; }
    }
}
