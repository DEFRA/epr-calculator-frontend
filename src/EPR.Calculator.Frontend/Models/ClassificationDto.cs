namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Data transfer object for calling the /v2/calculatorRuns endpoint.
    /// </summary>
    public record ClassificationDto
    {
        /// <summary>
        /// Gets the calculation run ID.
        /// </summary>
        public required int RunId { get; init; }

        /// <summary>
        /// Gets the classification ID to set the run to.
        /// </summary>
        public required int ClassificationId { get; init; }
    }
}
