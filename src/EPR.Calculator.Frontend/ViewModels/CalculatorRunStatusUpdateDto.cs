using EPR.Calculator.Frontend.Enums;

namespace EPR.Calculator.Frontend.ViewModels
{
    public class CalculatorRunStatusUpdateDto
    {
        public int RunId { get; set; }

        public int ClassificationId { get; set; }

        public string? CalcName { get; set; }
    }
}