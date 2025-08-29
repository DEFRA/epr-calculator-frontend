using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class FinancialYearClassificationResponseDto
    {
        public string FinancialYear { get; set; } = null!;

        public List<CalculatorRunClassificationDto> Classifications { get; set; } = null!;
    }
}
