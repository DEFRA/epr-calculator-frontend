using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Data Transfer Object (DTO) for representing errors related to creating default parameter settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateDefaultParameterSettingErrorDto
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// A string containing the error message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the error description.
        /// </summary>
        /// <value>
        /// A string containing a detailed description of the error.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the unique reference for the parameter.
        /// </summary>
        /// <value>
        /// A string containing the unique reference identifier for the parameter.
        /// </value>
        public string ParameterUniqueRef { get; set; }

        /// <summary>
        /// Gets or sets the category of the parameter.
        /// </summary>
        /// <value>
        /// A string containing the category of the parameter.
        /// </value>
        public string ParameterCategory { get; set; }

        /// <summary>
        /// Gets or sets the type of the parameter.
        /// </summary>
        /// <value>
        /// A string containing the type of the parameter.
        /// </value>
        public string ParameterType { get; set; }
    }
}
