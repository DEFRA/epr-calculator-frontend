using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// classify calculation run scenerio1 view model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record SetRunClassificationViewModel : ViewModelCommonData
    {
        /// <summary>
        /// gets or sets calculator run details.
        /// </summary>
        public required CalculatorRunDetailsViewModel CalculatorRunDetails { get; set; }

        /// <summary>
        /// Gets or sets the calssify run type.
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.ClassifyRunTypeNotSelected)]
        public int? ClassifyRunType { get; set; }

        public FinancialYearClassificationResponseDto? FinancialYearClassifications { get; set; }
    }
}