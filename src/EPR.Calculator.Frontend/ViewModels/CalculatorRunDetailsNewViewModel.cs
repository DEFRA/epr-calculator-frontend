﻿using EPR.Calculator.Frontend.Models;
using EPR.Calculator.Frontend.ViewModels.Enums;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.Frontend.ViewModels
{
    /// <summary>
    /// The view model for the calculator run Details page.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public record CalculatorRunDetailsNewViewModel : ViewModelCommonData
    {
        /// <summary>
        /// Gets the data for the run status update.
        /// </summary>
        public CalculatorRunDto Data { get; init; }

        /// <summary>
        /// Gets or sets download result URL.
        /// </summary>
        public Uri? DownloadResultURL { get; set; }

        /// <summary>
        /// Gets or sets download error URL.
        /// </summary>
        public string? DownloadErrorURL { get; set; }

        /// <summary>
        /// Gets or sets download Timeout.
        /// </summary>
        public int? DownloadTimeout { get; set; }

        /// <summary>
        /// Gets or sets the selected calculation run option.
        /// </summary>
        public CalculationRunOption? SelectedCalcRunOption { get; set; }

        /// <summary>
        /// Gets or Sets the error view model.
        /// </summary>
        public ErrorViewModel? Errors { get; set; }
    }
}
