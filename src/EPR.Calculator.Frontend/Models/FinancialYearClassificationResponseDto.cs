namespace EPR.Calculator.Frontend.Models
{
    public class FinancialYearClassificationResponseDto
    {
        public string FinancialYear { get; set; }

        public List<CalculatorRunClassificationDto> Classifications { get; set; }
    }
}
