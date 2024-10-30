using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Data Transfer Object (DTO) for creating default parameter settings.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class CreateDefaultParameterSettingDto
    {
        /// <summary>
        /// Gets or sets the parameter year.
        /// </summary>
        /// <value>
        /// The year for which the parameters are set.
        /// </value>
        public required string ParameterYear { get; set; }

        /// <summary>
        /// Gets or sets the collection of scheme parameter template values.
        /// </summary>
        /// <value>
        /// A collection of <see cref="SchemeParameterTemplateValue"/> objects representing the template values for the scheme parameters.
        /// </value>
        public required IEnumerable<SchemeParameterTemplateValue> SchemeParameterTemplateValues { get; set; }

        /// <summary>
        /// Gets or sets the parameter fielname.
        /// </summary>
        /// <value>
        /// The filename
        /// </value>
        public required string ParameterFileName { get; set; }
    }
}