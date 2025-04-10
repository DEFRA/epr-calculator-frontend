using EPR.Calculator.Frontend.Models;
using System.Text.Json.Serialization;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// parameter refresh view model.
    /// </summary>
    public record ParameterRefreshViewModel
    {
        /// <summary>
        /// Gets the collection of scheme parameter template values.
        /// </summary>
        /// <value>
        /// A collection of <see cref="SchemeParameterTemplateValue"/> objects representing the template values for the scheme parameters.
        /// </value>
        [JsonPropertyName("schemeParameterTemplateValues")]
        public required List<SchemeParameterTemplateValue> ParameterTemplateValues { get; init; }

        /// <summary>
        /// Gets the parameter file name.
        /// </summary>
        [JsonPropertyName("parameterFileName")]
        public required string FileName { get; init; }
    }
}
