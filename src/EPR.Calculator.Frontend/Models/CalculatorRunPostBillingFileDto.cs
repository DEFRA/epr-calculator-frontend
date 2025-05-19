using EPR.Calculator.Frontend.Enums;

namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Data transfer object for post billing file of a calculator run.
    /// </summary>
    public class CalculatorRunPostBillingFileDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the calculator run.
        /// </summary>
        public int RunId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the calculator run.
        /// </summary>
        public int ClassificationId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the calculator name.
        /// </summary>
        public string? CalcName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the created at.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the created by.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the financial year.
        /// </summary>
        public string? FinancialYear { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the run classification id.
        /// </summary>
        public RunClassification RunClassificationId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the billing file id.
        /// </summary>
        public int? BillingFileId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the billing csv file name.
        /// </summary>
        public string? BillingCsvFileName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the billing json file name.
        /// </summary>
        public string? BillingJsonFileName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the billing file created by.
        /// </summary>
        public string? BillingFileCreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the billing file authorised by.
        /// </summary>
        public string? BillingFileAuthorisedBy { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the billing file created date.
        /// </summary>
        public DateTime? BillingFileCreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the billing file authorised date.
        /// </summary>
        public DateTime? BillingFileAuthorisedDate { get; set; }
    }
}