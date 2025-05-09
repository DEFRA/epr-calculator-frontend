using System.Drawing.Printing;

namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Represents the data transfer object used to request the generation of a billing file
    /// based on a specific calculator run.
    /// </summary>
    public class GenerateBillingFileRequestDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the calculator run used for billing file generation.
        /// </summary>
        public int CalculatorRunId { get; set; }
    }
}
