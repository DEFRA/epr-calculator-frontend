namespace EPR.Calculator.Frontend.Models
{
    public class CalculatorRunStatusUpdateDto
    {
        public int RunId { get; set; }

        public int ClassificationId { get; set; }

        public string? CalcName { get; set; }

        public string? CreatedDate { get; set; }

        public string? CreatedTime { get; set; }

        public string? CreatedBy { get; set; }

        public string? FinancialYear { get; set; }
    }
}