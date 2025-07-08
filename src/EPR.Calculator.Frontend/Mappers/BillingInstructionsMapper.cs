using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.Mappers
{
    /// <summary>
    /// Provides mapping logic from billing instruction DTOs to view models, including enum and status conversions.
    /// </summary>
    public class BillingInstructionsMapper : IBillingInstructionsMapper
    {
        /// <summary>
        /// Maps a <see cref="ProducerBillingInstructionsResponseDto"/> and pagination request to a <see cref="BillingInstructionsViewModel"/>.
        /// </summary>
        /// <param name="billingData">The billing data response DTO.</param>
        /// <param name="request">The pagination request view model.</param>
        /// <param name="currentUser">The current user's name.</param>
        /// <returns>A populated <see cref="BillingInstructionsViewModel"/>.</returns>
        public BillingInstructionsViewModel MapToViewModel(
            ProducerBillingInstructionsResponseDto billingData,
            PaginationRequestViewModel request,
            string currentUser)
        {
            var organisations = billingData.Records.Select(x => new Organisation
            {
                Id = x.ProducerId,
                OrganisationName = x.ProducerName ?? string.Empty,
                OrganisationId = x.ProducerId,
                BillingInstruction = this.MapBillingInstruction(x.SuggestedBillingInstruction),
                InvoiceAmount = x.SuggestedInvoiceAmount,
                Status = this.MapBillingStatus(x.BillingInstructionAcceptReject),
            }).ToList();

            return new BillingInstructionsViewModel
            {
                CurrentUser = currentUser,
                CalculationRun = new CalculationRunForBillingInstructionsDto
                {
                    Id = billingData.CalculatorRunId,
                    Name = billingData.RunName ?? string.Empty,
                },
                TablePaginationModel = new PaginationViewModel
                {
                    Records = organisations,
                    CurrentPage = request.Page,
                    PageSize = request.PageSize,
                    TotalRecords = billingData.TotalRecords,
                    RouteName = "BillingInstructions_Index",
                    RouteValues = new Dictionary<string, object?>
                    {
                        { "calculationRunId", billingData.CalculatorRunId },
                    },
                },
            };
        }

        /// <summary>
        /// Maps a string value to a <see cref="BillingInstruction"/> enum value.
        /// </summary>
        /// <param name="suggested">The suggested billing instruction as a string.</param>
        /// <returns>The corresponding <see cref="BillingInstruction"/> enum value.</returns>
        private BillingInstruction MapBillingInstruction(string suggested)
        {
            if (string.IsNullOrWhiteSpace(suggested))
            {
                return BillingInstruction.Noaction;
            }

            var normalized = suggested.Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .ToLowerInvariant();

            return normalized switch
            {
                "noaction" => BillingInstruction.Noaction,
                "initial" => BillingInstruction.Initial,
                "delta" => BillingInstruction.Delta,
                "rebill" => BillingInstruction.Rebill,
                "cancelbill" => BillingInstruction.Cancelbill,
                _ => BillingInstruction.Noaction,
            };
        }

        private BillingStatus MapBillingStatus(string? acceptReject)
        {
            if (string.IsNullOrWhiteSpace(acceptReject))
            {
                return BillingStatus.Pending;
            }

            var normalized = acceptReject.Replace(" ", string.Empty)
                .Replace("-", string.Empty)
                .Replace("_", string.Empty)
                .ToLowerInvariant();

            return normalized switch
            {
                "noaction" => BillingStatus.Noaction,
                "accepted" => BillingStatus.Accepted,
                "rejected" => BillingStatus.Rejected,
                "pending" => BillingStatus.Pending,
                _ => BillingStatus.Noaction,
            };
        }
    }
}