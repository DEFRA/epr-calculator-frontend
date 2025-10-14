using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// calculation run classification status information view model.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record ClassificationStatusInformationViewModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets show initial run status description.
        /// </summary>
        public bool ShowInitialRunDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets show initial run status description.
        /// </summary>
        public string? InitialRunDescription { get; set; }

        /// <summary>
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets show interim recalculation run status description.
        /// </summary>
        public bool ShowInterimRecalculationRunDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets show interim recalculation run status description.
        /// </summary>
        public string? InterimRecalculationRunDescription { get; set; }

        /// <summary>
        /// <summary>
        /// Gets or sets a value indicating whether gets or sets show final recalculation run status description.
        /// </summary>
        public bool ShowFinalRecalculationRunDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets final recalculation run status description.
        /// </summary>
        public string? FinalRecalculationRunDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets show final run description.
        /// </summary>
        public bool ShowFinalRunDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets final run status description.
        /// </summary>
        public string? FinalRunDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether gets or sets show final run description.
        /// </summary>
        public bool ShowTestRunDescription { get; set; }

        /// <summary>
        /// Gets or sets a value Test informatopm if try to classify run on wrong date
        /// </summary>
        public string? TestRunDescription { get; set; }
    }
}