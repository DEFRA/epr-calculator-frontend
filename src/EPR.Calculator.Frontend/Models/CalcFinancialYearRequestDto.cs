namespace EPR.Calculator.Frontend.Models
{
    public class CalcFinancialYearRequestDto
    {
        public int RunId { get; set; }

        public required string FinancialYear { get; set; }
    }
}
