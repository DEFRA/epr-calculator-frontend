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
        /// Gets or sets the access token to authenticate the API.
        /// </summary>
        public required string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the financial year.
        /// </summary>
        public required string FinancialYear { get; set; }

        /// <summary>
        /// Gets or sets the financial year list API.
        /// </summary>
        public string? FinancialYearListApi { get; set; }

        /// <summary>
        /// Gets or sets the calculation run list.
        /// </summary>
        public IEnumerable<CalculationRunViewModel>? Calculations { get; set; }
    }
}
