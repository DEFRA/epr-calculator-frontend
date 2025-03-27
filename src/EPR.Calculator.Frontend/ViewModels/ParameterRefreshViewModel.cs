using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// parameter refresh view model.
    /// </summary>
    public record ParameterRefreshViewModel
    {
        /// <summary>
        /// Gets or Sets the error view model.
        /// </summary>
        public List<SchemeParameterTemplateValue> ParameterTemplateValue { get; set; }

        /// <summary>
        /// Gets or Sets the parameter file name.
        /// </summary>
        public string FileName { get; set; }
    }
}
