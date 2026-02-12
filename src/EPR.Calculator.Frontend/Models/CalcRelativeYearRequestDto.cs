namespace EPR.Calculator.Frontend.Models
{
    public class CalcRelativeYearRequestDto
    {
        public required int RunId { get; set; }

        public required RelativeYear RelativeYear { get; set; }
    }
}
