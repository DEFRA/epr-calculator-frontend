namespace EPR.Calculator.Frontend.Models
{
    public class CalculatorRunParamsDto
    {
        public required string FinancialYear { get; init; }

        public static explicit operator CalculatorRunParamsDto(string financialYear)
            => new CalculatorRunParamsDto { FinancialYear = financialYear };
    }
}