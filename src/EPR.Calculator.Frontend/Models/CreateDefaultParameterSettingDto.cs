using EPR.Calculator.Frontend.ViewModels;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.Models
{
    /// <summary>
    /// Data Transfer Object (DTO) for creating default parameter settings.
    /// </summary>
    public record CreateDefaultParameterSettingDto : ParameterRefreshViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateDefaultParameterSettingDto"/> class.
        /// </summary>
        public CreateDefaultParameterSettingDto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateDefaultParameterSettingDto"/> class.
        /// </summary>
        /// <param name="original">
        /// The <see cref="ParameterRefreshViewModel"/> to copy the parameter list and file name from.
        /// </param>
        /// <param name="parameterYear">The parameter year.</param>
        [SetsRequiredMembers]
        public CreateDefaultParameterSettingDto(
            ParameterRefreshViewModel original,
            string parameterYear)
        {
            this.ParameterTemplateValues = original.ParameterTemplateValues;
            this.FileName = original.FileName;
            this.ParameterYear = parameterYear;
        }

        /// <summary>
        /// Gets the parameter year.
        /// </summary>
        /// <value>
        /// The year for which the parameters are set.
        /// </value>
        public required string ParameterYear { get; init; }
    }
}