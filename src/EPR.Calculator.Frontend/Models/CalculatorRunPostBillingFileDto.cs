using EPR.Calculator.Frontend.Enums;

namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Data transfer object for post billing file of a calculator run.
    /// </summary>
    public record CalculatorRunPostBillingFileDto
    {
        /// <summary>
        /// Gets the unique identifier of the calculator run.
        /// </summary>
        public int RunId { get; init; }

        /// <summary>
        /// Gets the unique identifier of the calculator run.
        /// </summary>
        public int ClassificationId { get; init; }

        /// <summary>
        /// Gets the unique identifier of the calculator name.
        /// </summary>
        public string CalcName { get; init; }

        /// <summary>
        /// Gets the unique identifier of the created at.
        /// </summary>
        public DateTime CreatedAt { get; init; }

        /// <summary>
        /// Gets the unique identifier of the created by.
        /// </summary>
        public string? CreatedBy { get; init; }

        /// <summary>
        /// Gets the unique identifier of the financial year.
        /// </summary>
        public string FinancialYear { get; init; }

        /// <summary>
        /// Gets the unique identifier of the run classification id.
        /// </summary>
        public RunClassification RunClassificationId { get; init; }

        /// <summary>
        /// Gets the unique identifier of the billing file id.
        /// </summary>
        public int? BillingFileId { get; init; }

        /// <summary>
        /// Gets the unique identifier of the billing csv file name.
        /// </summary>
        public string? BillingCsvFileName { get; init; }

        /// <summary>
        /// Gets the unique identifier of the billing json file name.
        /// </summary>
        public string? BillingJsonFileName { get; init; }

        /// <summary>
        /// Gets the unique identifier of the billing file created by.
        /// </summary>
        public string? BillingFileCreatedBy { get; init; }

        /// <summary>
        /// Gets the unique identifier of the billing file authorised by.
        /// </summary>
        public string? BillingFileAuthorisedBy { get; init; }

        /// <summary>
        /// Gets the unique identifier of the billing file created date.
        /// </summary>
        public DateTime? BillingFileCreatedDate { get; init; }

        /// <summary>
        /// Gets the unique identifier of the billing file authorised date.
        /// </summary>
        public DateTime? BillingFileAuthorisedDate { get; init; }
    }
}