using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.Mappers
{
    /// <summary>
    /// Defines a contract for mapping billing instruction data transfer objects to view models.
    /// </summary>
    public interface IBillingInstructionsMapper
    {
        /// <summary>
        /// Maps a <see cref="ProducerBillingInstructionsResponseDto"/> and pagination request to a <see cref="BillingInstructionsViewModel"/>.
        /// </summary>
        /// <param name="billingData">The billing data response DTO.</param>
        /// <param name="request">The pagination request view model.</param>
        /// <param name="currentUser">The current user's name.</param>
        /// <returns>A populated <see cref="BillingInstructionsViewModel"/>.</returns>
        BillingInstructionsViewModel MapToViewModel(
            ProducerBillingInstructionsResponseDto? billingData,
            PaginationRequestViewModel request,
            string currentUser,
            bool isSelectAll,
            bool isSelectAllPage);
    }
}