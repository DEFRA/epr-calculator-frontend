namespace EPR.Calculator.Frontend.ViewModels
{
    public record PaginationRequestViewModel
    {
        public int Page { get; init; } = 1;

        public int PageSize { get; init; } = 10;
    }
}
