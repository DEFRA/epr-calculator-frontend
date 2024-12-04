using EPR.Calculator.Frontend.Enums;

namespace EPR.Calculator.Frontend.ViewModels
{
    public class CalculatorRunStatusUpdateViewModel
    {
        public int RunId { get; set; }
        public int ClassificationId = (int)RunClassification.DELETED;
    }
}