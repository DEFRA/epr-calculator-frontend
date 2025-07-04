using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.ViewModels;
using System.ComponentModel;

namespace EPR.Calculator.Frontend.Models
{
    public record CalculationRunOrganisationBillingInstructionsDTO
    {
        public CalculationRunForBillingInstructionsDTO CalculationRun { get; init; }

        public ICollection<Organisation> Organisations { get; init; } = new List<Organisation>();
    }

    public record Organisation
    {
        public int Id { get; init; }

        public string? OrganisationName { get; init; }

        public int OrganisationId { get; init; }

        public BillingInstruction? BillingInstruction { get; init; }

        public double InvoiceAmount { get; init; }

        public BillingStatus? Status { get; init; }
    }
}
