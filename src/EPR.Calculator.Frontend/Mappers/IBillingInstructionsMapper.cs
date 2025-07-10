using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.Mappers
{
    public interface IBillingInstructionsMapper
    {
        BillingInstructionsViewModel MapToViewModel(
            ProducerBillingInstructionsResponseDto billingData,
            PaginationRequestViewModel request,
            string currentUser,
            bool isSelectAll,
            bool isSelectAllPage);
    }
}