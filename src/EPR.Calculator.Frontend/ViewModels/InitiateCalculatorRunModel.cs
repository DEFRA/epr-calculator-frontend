using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]
    public record InitiateCalculatorRunModel : ViewModelCommonData
    {
        public string FinancialYear { get; set; }

        public string? CalculationName { get; set; }
    }
}