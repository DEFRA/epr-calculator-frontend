namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Represents a classified calculator run with its metadata.
    /// </summary>
    public class ClassifiedCalculatorRunDto
    {
        /// <summary>
        /// Gets the unique identifier for the calculator run.
        /// </summary>
        public int RunId { get; init; }

        /// <summary>
        /// Gets the date and time when the run was created.
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// Gets the name of the calculator run.
        /// </summary>
        public required string RunName { get; init; }

        /// <summary>
        /// Gets the classification identifier for the run.
        /// </summary>
        public int RunClassificationId { get; init; }

        /// <summary>
        /// Gets the date and time when the run was last updated, if available.
        /// </summary>
        public DateTime? UpdatedAt { get; init; }
    }
}