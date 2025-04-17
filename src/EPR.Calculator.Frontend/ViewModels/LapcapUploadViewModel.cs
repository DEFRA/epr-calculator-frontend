using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// lapcap upload view model.
    /// </summary>
    public record LapcapUploadViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or Sets the lapcap errors.
        /// </summary>
        public List<CreateLapcapDataErrorDto>? LapcapErrors { get; set; }

        /// <summary>
        /// Gets or Sets the lapcap validation errors.
        /// </summary>
        public List<ValidationErrorDto>? ValidationErrors { get; set; }

        /// <summary>
        /// Gets or Sets the lapcap data template value.
        /// </summary>
        public List<LapcapDataTemplateValueDto>? LapcapDataTemplateValue { get; set; }
    }
}
