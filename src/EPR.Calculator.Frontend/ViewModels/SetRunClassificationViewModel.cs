using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// classify calculation run scenerio view model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record SetRunClassificationViewModel : ViewModelCommonData
    {
        /// <summary>
        /// gets or sets calculator run details.
        /// </summary>
        public required CalculatorRunDetailsViewModel CalculatorRunDetails { get; set; }

        /// <summary>
        /// Gets or sets the classify run type.
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.ClassifyRunTypeNotSelected)]
        public int? ClassifyRunType { get; set; }

        /// <summary>
        /// Gets or sets the selected calculation run option.
        /// </summary>
        public string? SelectedCalcRunOption { get; set; }

        public FinancialYearClassificationResponseDto? FinancialYearClassifications { get; set; }

        /// <summary>
        /// Gets or sets the classification status information toggles.
        /// </summary>
        public ClassificationStatusInformationViewModel? ClassificationStatusInformation { get; set; }

        /// <summary>
        /// Gets or sets the Importanti information information box.
        /// </summary>
        public ImportantSectionViewModel ImportantiewModel { get; set; } = new ImportantSectionViewModel();
    }
}