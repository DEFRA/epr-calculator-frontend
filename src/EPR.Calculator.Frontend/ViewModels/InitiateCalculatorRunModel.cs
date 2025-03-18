using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]
    public record InitiateCalculatorRunModel : ViewModelCommonData
    {
        public string? CalculationName { get; set; }
    }
}