using EPR.Calculator.Frontend.Constants;
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
    public record ClassifyCalculationRunScenerio1ViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets or sets the calculator run id.
        /// </summary>
        [Required]
        public int RunId { get; set; }

        /// <summary>
        /// Gets or sets the created by user.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the calculation run was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the Run name.
        /// </summary>
        public string? RunName { get; set; }

        /// <summary>
        /// Gets or sets the financial year.
        /// </summary>
        public string? FinancialYear { get; set; }

        /// <summary>
        /// Gets or sets the calssify run type.
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.ClassifyRunTypeNotSelected)]
        public ClassifyRunType? ClassifyRunType { get; set; }
    }
}