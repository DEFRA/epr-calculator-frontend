using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.ViewModels;
using System.ComponentModel;

namespace EPR.Calculator.Frontend.Models
{
    public record CalculationRunOrganisationBillingInstructionsDto
    {
        public CalculationRunForBillingInstructionsDto CalculationRun { get; init; } = new();

        public ICollection<Organisation> Organisations { get; init; } = new List<Organisation>();
    }
}
