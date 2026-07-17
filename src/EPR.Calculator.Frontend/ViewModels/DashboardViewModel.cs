using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EPR.Calculator.Frontend.ViewModels;

public record DashboardViewModel
{
    public required RelativeYear RelativeYear { get; init; }
    public required List<CalculationRunViewModel> Calculations { get; init; }
    public required List<SelectListItem> RelativeYearSelectList { get; init; }
}
