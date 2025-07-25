using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;

namespace EPR.Calculator.Frontend.ViewModels
{
    public record PaginationRequestViewModel
    {
        public int Page { get; init; } = CommonConstants.DefaultPage;

        public int PageSize { get; init; } = CommonConstants.DefaultPageSize;

        public int? OrganisationId { get; init; }

        public BillingStatus? BillingStatus { get; init; }
    }
}