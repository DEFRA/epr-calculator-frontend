using EPR.Calculator.Frontend.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// View model used to hold the data for the dashboard page.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record DashboardViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or sets the relative year.
        /// </summary>
        public required RelativeYear RelativeYear { get; set; }

        /// <summary>
        /// Gets or sets the calculation run list.
        /// </summary>
        public IEnumerable<CalculationRunViewModel>? Calculations { get; set; }

        /// <summary>
        /// Gets or sets the relative years.
        /// </summary>
        public List<SelectListItem>? RelativeYearSelectList { get; set; }
    }
}
