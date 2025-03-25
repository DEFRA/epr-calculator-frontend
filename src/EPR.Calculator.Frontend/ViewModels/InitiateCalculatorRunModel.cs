using EPR.Calculator.Frontend.Models;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    [ExcludeFromCodeCoverage]
    public record InitiateCalculatorRunModel : ViewModelCommonData
    {
        public string? CalculationName { get; set; }

        public ErrorViewModel? Errors { get; set; }

        public string BackLink { get; set; } = "Dashboard";
    }
}