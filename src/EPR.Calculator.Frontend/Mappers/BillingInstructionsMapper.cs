using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels;

namespace EPR.Calculator.Frontend.Mappers
{
    public class BillingInstructionsMapper : IBillingInstructionsMapper
    {
        public BillingInstructionsViewModel MapToViewModel(
            ProducerBillingInstructionsResponseDto billingData,
            PaginationRequestViewModel request,
            string currentUser,
            bool isSelectAll)
        {
            var organisations = billingData.Records.Select(x => new Organisation
            {
                Id = x.ProducerId,
                OrganisationName = x.ProducerName ?? string.Empty,
                OrganisationId = x.ProducerId,
                BillingInstruction = MapBillingInstruction(x.SuggestedBillingInstruction),
                InvoiceAmount = x.SuggestedInvoiceAmount,
                Status = MapBillingStatus(x.BillingInstructionAcceptReject),
            }).ToList();

            return new BillingInstructionsViewModel
            {
                CurrentUser = currentUser,
                CalculationRun = new CalculationRunForBillingInstructionsDto
                {
                    Id = billingData.CalculatorRunId,
                    Name = billingData.RunName ?? string.Empty,
                },
                OrganisationBillingInstructions = organisations,
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
                ProducerIds = billingData.AllProducerIds,
                OrganisationSelections = new OrganisationSelectionsViewModel { SelectAll = isSelectAll },
            };
        }

        private BillingInstruction MapBillingInstruction(string suggested)
        {
            if (string.IsNullOrWhiteSpace(suggested))
                return BillingInstruction.Noaction;

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
                _ => BillingInstruction.Noaction
            };
        }

        private BillingStatus MapBillingStatus(string? acceptReject)
        {
            if (string.IsNullOrWhiteSpace(acceptReject))
                return BillingStatus.Pending;

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
                _ => BillingStatus.Noaction
            };
        }
    }
}