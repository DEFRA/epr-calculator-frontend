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
        /// Gets or sets the financial year.
        /// </summary>
        public required string FinancialYear { get; set; }

        /// <summary>
        /// Gets or sets the calculation run list.
        /// </summary>
        public IEnumerable<CalculationRunViewModel>? Calculations { get; set; }

        /// <summary>
        /// Gets or sets the financial years.
        /// </summary>
        public List<SelectListItem>? FinancialYearSelectList { get; set; }

        /// <summary>
        /// Sets the currently selected financial year.
        /// </summary>
        public string SelectedFinancialYear { get; set; }
    }
}
