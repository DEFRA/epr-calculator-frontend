using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    [ExcludeFromCodeCoverage]
    public class ErrorDto
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// A string containing the error message.
        /// </value>
        public required string Message { get; set; }

        /// <summary>
        /// Gets or sets the error description.
        /// </summary>
        /// <value>
        /// A string containing a detailed description of the error.
        /// </value>
        public string Description { get; set; } = string.Empty;
    }
}
