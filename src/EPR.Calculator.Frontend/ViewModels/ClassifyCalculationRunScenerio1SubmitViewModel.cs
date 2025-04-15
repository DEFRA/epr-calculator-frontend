using EPR.Calculator.Frontend.Constants;
using EPR.Calculator.Frontend.Enums;
using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// classify calculation run scenerio1 view model.
    /// </summary>
    public record ClassifyCalculationRunScenerio1SubmitViewModel
    {
        /// <summary>
        ///  Gets or sets run Id.
        /// </summary>
        public int RunId { get; set; }

        /// <summary>
        /// Gets or sets classify run type.
        /// </summary>
        [Required(ErrorMessage = ErrorMessages.ClassifyRunTypeNotSelected)]
        public ClassifyRunType? ClassifyRunType { get; set; }
    }
}