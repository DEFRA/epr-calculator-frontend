using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EPR.Calculator.Frontend.ViewModels;

public record DashboardViewModel : ViewModelCommonData
{
    public required RelativeYear RelativeYear { get; set; }
    public IEnumerable<CalculationRunViewModel>? Calculations { get; set; }
    public List<SelectListItem>? RelativeYearSelectList { get; set; }
}
