using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.Frontend.Enums;

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
        public RelativeYear RelativeYear { get; init; }

        /// <summary>
        /// Gets or sets the list of classifications for calculator runs.
        /// </summary>
        public ImmutableList<RunClassification> Classifications { get; init; }  = [];

        /// <summary>
        /// Gets or sets the list of classified calculator runs.
        /// </summary>
        public ImmutableList<CalculatorRunDto> ClassifiedRuns { get; init; } = [];
    }
}
