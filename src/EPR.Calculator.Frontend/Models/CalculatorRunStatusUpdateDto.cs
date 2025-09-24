namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Represents the data transfer object for updating the status of a calculator run.
    /// </summary>
   public class CalculatorRunStatusUpdateDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the calculator run.
        /// </summary>
        public int RunId { get; set; }

        /// <summary>
        /// Gets or sets the classification identifier associated with the calculator run.
        /// </summary>
        public int ClassificationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the calculator.
        /// </summary>
        public string? CalcName { get; set; }

        /// <summary>
        /// Gets or sets the date when the calculator run was created.
        /// </summary>
        public string? CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the time when the calculator run was created.
        /// </summary>
        public string? CreatedTime { get; set; }

        /// <summary>
        /// Gets or sets the time when the calculator run was last updated.
        /// </summary>
        public string? UpdatedTime { get; set; }

        /// <summary>
        /// Gets or sets the financial year associated with the calculator run.
        /// </summary>
        public string? FinancialYear { get; set; }
    }
}