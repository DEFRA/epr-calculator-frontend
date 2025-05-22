namespace EPR.Calculator.Frontend.Models
{
    public class PaginatedTableViewModel
    {
        public IEnumerable<object> Records { get; set; }

        public string Caption { get; set; }

        public string FormAction { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalRecords { get; set; }

        public string BaseUrl { get; set; }

        public IEnumerable<int> PageSizeOptions { get; set; } = new[] { 10, 25, 50 };

        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
}
