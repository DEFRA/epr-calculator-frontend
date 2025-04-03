using EPR.Calculator.Frontend.Models;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// lapcap refresh view model.
    /// </summary>
    public record LapcapRefreshViewModel
    {
        /// <summary>
        /// Gets or Sets the lapcap data template value view model.
        /// </summary>
        public List<LapcapDataTemplateValueDto> LapcapTemplateValue { get; set; }

        /// <summary>
        /// Gets or Sets the lapcap data file name.
        /// </summary>
        public string FileName { get; set; }
    }
}
