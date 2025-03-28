using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// parameter upload view model.
    /// </summary>
    public record ParameterUploadViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or Sets the error view model.
        /// </summary>
        public ErrorViewModel? Errors { get; set; }

        /// <summary>
        /// Gets or Sets the lapcap errors.
        /// </summary>
        public List<CreateDefaultParameterSettingErrorDto>? ParamterErrors { get; set; }

        /// <summary>
        /// Gets or Sets the lapcap validation errors.
        /// </summary>
        public List<ValidationErrorDto>? ValidationErrors { get; set; }

        /// <summary>
        /// Gets or Sets the lapcap data template value.
        /// </summary>
        public List<SchemeParameterTemplateValue>? ParameterDataTemplateValue { get; set; }
    }
}
