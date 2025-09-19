namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Represents the classification response for a financial year, including classifications and classified runs.
    /// </summary>
    public class FinancialYearClassificationResponseDto
    {
        /// <summary>
        /// Gets or sets the financial year for which the classifications apply.
        /// </summary>
        public required string FinancialYear { get; set; }

        /// <summary>
        /// Gets or sets the list of classifications for calculator runs.
        /// </summary>
        public required List<CalculatorRunClassificationDto> Classifications { get; set; }

        /// <summary>
        /// Gets or sets the list of classified calculator runs.
        /// </summary>
        public List<ClassifiedCalculatorRunDto> ClassifiedRuns { get; set; } = [];
    }
}