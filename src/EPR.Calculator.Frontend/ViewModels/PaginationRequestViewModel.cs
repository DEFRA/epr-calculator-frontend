using EPR.Calculator.Frontend.Constants;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record PaginationRequestViewModel
    {
        public int Page { get; init; } = CommonConstants.DefaultPage;

        public int PageSize { get; init; } = CommonConstants.DefaultPageSize;

        public int? OrganisationId { get; init; }
    }
}