using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels;

public record DashboardViewModel
{
    public required RelativeYear RelativeYear { get; init; }
    public required List<CalculationRunViewModel> Calculations { get; init; }
    public required bool HasDefaultParameters { get; init; }
    public required bool HasLocalAuthorityDisposalCosts { get; init; }

    public bool CanRunCalculation => HasDefaultParameters && HasLocalAuthorityDisposalCosts;
}
