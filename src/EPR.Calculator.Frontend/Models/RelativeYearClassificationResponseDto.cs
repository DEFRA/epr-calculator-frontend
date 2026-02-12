using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Represents the classification response for a relative year, including classifications and classified runs.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RelativeYearClassificationResponseDto
    {
        /// <summary>
        /// Gets or sets the relative year for which the classifications apply.
        /// </summary>
        public required RelativeYear RelativeYear { get; set; }

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